using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using FFmpeg.NET;
using FFMpegCore;
using FFMpegCore.Helpers;
using FFMpegCore.Enums;
using System.Diagnostics;

namespace Beeper
{
    public class FileProcessor : PropertySensitive
    {
        Control ctx;
        public string Name { get => GetPar(""); private set => SetPar(value); }
        public string Status { get => GetPar("Starting"); private set => SetPar(value); }
        public FileProcessor(Control ctx)
        {
            this.ctx = ctx;
        }
        public async void Start(Settings settings, string inputFile, string outputFile, CancellationToken token = default)
        {
            Name = Path.GetFileNameWithoutExtension(inputFile);
            string audioFile = outputFile + ".mp3";
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, null, token);
            await GenerateAudioAsync(settings, audioFile, mediaInfo.Duration);

            FFMpegHelper.ConversionSizeExceptionCheck(FFProbe.Analyse(inputFile));

            await FFMpegArguments
                .FromFileInput(inputFile)
                .AddFileInput(audioFile)
                .OutputToFile(outputFile, overwrite: true, options=>options
                    .CopyChannel(Channel.Video)
                    //.WithAudioCodec(AudioCodec.Aac)
                    //.WithAudioBitrate(AudioQuality.Good)
                    .UsingShortest(false))
                .NotifyOnProgress(a => Progress(a, mediaInfo.Duration))
                .ProcessAsynchronously();
            File.Delete(audioFile);
            Status = "Done";
        }

        void Progress(TimeSpan progress, TimeSpan total)
        {
            double p = (double)progress.TotalSeconds * 100 / (double)total.TotalSeconds;
            if (p < 0 || p > 100)
            {
                ctx.InvokeIfRequired(() =>
                {
                    Status = $"Running ";

                });
            }
            else
            {
                ctx.InvokeIfRequired(() =>
                {
                    Status = $"Running {p.ToString("0.00")}%";

                });
            }
        }

        async Task GenerateAudioAsync(Settings settings, string outputFile, TimeSpan length)
        {
            await Task.Run(()=>{
                ISampleProvider signalGenerator = new SignalGenerator() { Frequency = settings.Tone, Gain = settings.Gain };
                ISampleProvider silenceProvider = new SilenceProvider(signalGenerator.WaveFormat).ToSampleProvider();

                TimeSpan t = settings.Offset;
                ISampleProvider sampleProvider;
                bool doBeep = true;

                if (settings.Offset > TimeSpan.Zero)
                {
                    sampleProvider = silenceProvider.Take(settings.Offset);
                    doBeep = true;
                }
                else
                {
                    sampleProvider = signalGenerator.Take(settings.Duration);
                    doBeep = false;
                }

                while (t < length)
                {
                    if (doBeep)
                    {
                        sampleProvider = sampleProvider.FollowedBy(signalGenerator.Take(settings.Duration));
                        t += settings.Duration;
                    }
                    else
                    {
                        if(t + settings.Interval < length)
                            sampleProvider = sampleProvider.FollowedBy(silenceProvider.Take(settings.Interval));
                        t += settings.Interval;
                    }
                    doBeep = !doBeep;
                }
                // optionally truncate to video length
                //var truncated = sampleProvider.Take(length);

                // go back down to 16 bit PCM 
                var converted16Bit = new SampleToWaveProvider16(sampleProvider);
                // now for MP3, we need to upsample to 44.1kHz. Use MediaFoundationResampler 
                using (var resampled = new MediaFoundationResampler(converted16Bit, new WaveFormat(44100, 1)))
                {
                    var desiredBitRate = 0; // ask for lowest available bitrate 
                    MediaFoundationEncoder.EncodeToMp3(resampled, outputFile, desiredBitRate);
                }


                //WaveFileWriter.CreateWaveFile16(outputFile, converted16Bit);
            });
        }

        public override string ToString()
        {
            return $"{Name} - {Status}";
        }
    }
}
