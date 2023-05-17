using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using FFmpeg.NET;
using FFMpegCore;

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
            string audioFile = outputFile + ".wav";
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, null, token);
            await GenerateAudioAsync(settings, audioFile, mediaInfo.Duration);
            var ffmpeg = new Engine("ffmpeg.exe");
            ffmpeg.Progress += Ffmpeg_Progress;
            await ffmpeg.ExecuteAsync($"-i {inputFile} -i {audioFile} -c:v copy -map 0:v:0 -map 1:a:0 {outputFile}", token);
            File.Delete(audioFile);
            Status = "Done";
        }

        private void Ffmpeg_Progress(object? sender, FFmpeg.NET.Events.ConversionProgressEventArgs e)
        {
            double p = (double)e.ProcessedDuration.TotalMinutes * 100 / (double)e.MediaInfo.TotalDuration.TotalSeconds;

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
                ISampleProvider sampleProvider = silenceProvider.Take(settings.Offset);
                bool doBeep = true;
                while (t < length)
                {
                    if (doBeep)
                    {
                        sampleProvider = sampleProvider.FollowedBy(signalGenerator.Take(settings.Duration));
                        t += settings.Duration;
                    }
                    else
                    {
                        if (t + settings.Interval < length)
                        {
                            sampleProvider = sampleProvider.FollowedBy(silenceProvider.Take(settings.Interval));
                        }
                        t += settings.Interval;
                    }
                    doBeep = !doBeep;
                }
                WaveFileWriter.CreateWaveFile16(outputFile, sampleProvider);
            });
        }

        public override string ToString()
        {
            return $"{Name} - {Status}";
        }
    }
}
