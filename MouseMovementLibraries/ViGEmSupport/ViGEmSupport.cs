using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Timers;

namespace MouseMovementLibraries.ViGEmSupport
{
    internal class ViGEmInput
    {
        private readonly ViGEmClient client;
        private readonly IXbox360Controller controller;
        private readonly System.Timers.Timer resetTimer;

        public ViGEmInput()
        {
            try
            {
                client = new ViGEmClient();
                controller = client.CreateXbox360Controller();
                controller.Connect();

                resetTimer = new System.Timers.Timer(200); // 200ms timeout
                resetTimer.Elapsed += ResetController;
                resetTimer.AutoReset = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize ViGEmBus: {ex.Message}\nMake sure ViGEmBus driver is installed.", "ViGEm Error");
                throw;
            }
        }

        public void SendMouseCommand(int x, int y, int click, int xSensitivity = 150, int ySensitivity = 150)
        {
            if (controller == null) return;

            // Reset the timer
            resetTimer.Stop();
            resetTimer.Start();

            short thumbX = MapToStick(x, xSensitivity);
            short thumbY = MapToStick(-y, ySensitivity); // Invert Y

            try
            {
                controller.SetAxisValue(Xbox360Axis.RightThumbX, thumbX);
                controller.SetAxisValue(Xbox360Axis.RightThumbY, thumbY);

                if (click == 1)
                    controller.SetSliderValue(Xbox360Slider.RightTrigger, 255);
                else
                    controller.SetSliderValue(Xbox360Slider.RightTrigger, 0);
            }
            catch { }
        }

        private short MapToStick(int input, int sensitivity)
        {
            if (input == 0) return 0;

            const int DEADZONE = 4000; 
            const int MAX_VAL = 32767;

            int absInput = Math.Abs(input);
            int mapped = DEADZONE + (absInput * sensitivity);

            if (input < 0) return (short)Math.Max(-MAX_VAL, -mapped);
            return (short)Math.Min(MAX_VAL, mapped);
        }
        
        private void ResetController(object? sender, ElapsedEventArgs e)
        {
             try {
                controller.SetAxisValue(Xbox360Axis.RightThumbX, 0);
                controller.SetAxisValue(Xbox360Axis.RightThumbY, 0);
                controller.SetSliderValue(Xbox360Slider.RightTrigger, 0);
             } catch { }
        }

        public void Close()
        {
            resetTimer?.Stop();
            resetTimer?.Dispose();
            controller?.Disconnect();
            client?.Dispose();
        }
    }
}
