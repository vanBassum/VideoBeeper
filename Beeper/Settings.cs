using System.ComponentModel;

namespace Beeper
{
    public class Settings
    {
        [Description("How long into the video, the first beep should occur.")]
        public TimeSpan Offset { get; set; } = TimeSpan.FromMinutes(15);
        [Description("Add a beep every x time.")]
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);  
        [Description("How long the beep should be.")]
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(2);
        [Description("The frequency of the beep in hertz.")]
        public int Tone { get; set; } = 1000;
        [Description("Volume of the beep.")]
        public float Gain { get; set; } = 1f;
    }
}
