using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VentilatorTestConsole
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PatientStatusView : ContentPage
	{
        Stopwatch stopwatch = new Stopwatch();
        bool pageIsActive;
        float MaxVol;
        float MaxPress;
        float MinPress;
        PatientStatus status;

        SKPaint VolPaint;
        SKPaint PressPaint;

        public PatientStatusView (Patient p)
        {
            if (p == Patient.A)
            {
                status = (Application.Current as App).StatService.Patient1;
            } else
            {
                status = (Application.Current as App).StatService.Patient2;
            }
            MaxVol = 0.25F;
            MaxPress = 111200;
            MinPress = 110999;
            VolPaint = new SKPaint { Color = SKColors.Blue };
            PressPaint = new SKPaint { Color = SKColors.Green };
            InitializeComponent ();
		}

        private void VolumeDisplay_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            int h = info.Height;
            int w = info.Width;
            int bound = status.RecentVolumMeasurements.Capacity() - 1;

            float wIncr = w / status.RecentVolumMeasurements.Capacity();
            float thisMax = 0;

            float startVal = status.RecentVolumMeasurements.Get(0);
            if (startVal > thisMax)
            {
                thisMax = startVal;
            }
            float startY = startVal / (MaxVol * 1.1F);
            startY = h - (startY * h);
            float startX = w;

            for (int i = 0; i < bound; i++)
            {
                float endX = startX - wIncr;

                float endVal = status.RecentVolumMeasurements.Get(i + 1);
                if (endVal > thisMax)
                {
                    thisMax = endVal;
                }

                float endY = endVal / (MaxVol * 1.1F);
                endY = h - (endY * h);

                canvas.DrawLine(startX, startY-5, endX, endY-5, VolPaint);

                startX = endX;
                startY = endY;
            }
            canvas.RotateDegrees(-90);
            canvas.DrawText("Volume (L)", new SKPoint(-h/2, 10), new SKPaint { Color = SKColors.Black });
            canvas.RotateDegrees(90);
            canvas.DrawText($"{((int)(MaxVol * 10))/10f}", new SKPoint(10, 10), new SKPaint { Color = SKColors.Black });
            canvas.DrawText("0", new SKPoint(10, h-10), new SKPaint { Color = SKColors.Black });

            canvas.DrawText("Time", new SKPoint(w / 2, h), new SKPaint { Color = SKColors.Black });
            canvas.DrawText("Now", new SKPoint(20, h), new SKPaint { Color = SKColors.Black });
            canvas.DrawText("Previously", new SKPoint(w - 60, h), new SKPaint { Color = SKColors.Black });

            canvas.DrawText($"TV: {((int)(status.TV * 100)) / 100f}", new SKPoint(w / 2, 15), new SKPaint { Color = SKColors.Black });
            canvas.DrawText($"I/E: {((int)(status.IE * 100)) / 100f}", new SKPoint(w / 2, 30), new SKPaint { Color = SKColors.Black });


            MaxVol = Math.Max(0.25F, (MaxVol + thisMax) / 2);
        }

        async Task AnimationLoop()
        {
            stopwatch.Start();

            while (pageIsActive)
            {
                VolumeDisplay.InvalidateSurface();
                PressureDisplay.InvalidateSurface();
                await Task.Delay(TimeSpan.FromSeconds(1.0 / 15));
            }

            stopwatch.Stop();
        }

        private void PressureDisplay_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            int h = info.Height;
            int w = info.Width;
            int bound = status.RecentPressMeasurements.Capacity() - 1;

            float wIncr = w / status.RecentPressMeasurements.Capacity();
            float thisMax = 0;
            float thisMin = float.MaxValue;

            float startVal = status.RecentPressMeasurements.Get(0);
            if (startVal > thisMax)
            {
                thisMax = startVal;
            }
            if (startVal < thisMin)
            {
                thisMin = startVal;
            }

            float divDiff = MaxPress - MinPress;

            float startY = (startVal - MinPress) / (divDiff*1.1f);
            startY = h - (startY * h);
            float startX = w;

            for (int i = 0; i < bound; i++)
            {
                float endX = startX - wIncr;

                float endVal = status.RecentPressMeasurements.Get(i + 1);
                if (endVal > thisMax)
                {
                    thisMax = endVal;
                }

                float endY = (endVal - MinPress) / (divDiff * 1.1f);
                endY = h - (endY * h);

                canvas.DrawLine(startX, startY-5, endX, endY-5, PressPaint);

                startX = endX;
                startY = endY;

                if (startVal < thisMin)
                {
                    thisMin = startVal;
                }
            }
            canvas.RotateDegrees(-90);
            canvas.DrawText("Pressure (kPa)", new SKPoint(-h / 2, 10), new SKPaint { Color = SKColors.Black });
            canvas.RotateDegrees(90);
            canvas.DrawText($"{((int)(MaxPress * 10)) / 10000f}", new SKPoint(10, 10), new SKPaint { Color = SKColors.Black });
            canvas.DrawText($"{((int)(MinPress * 10)) / 10000f}", new SKPoint(10, h - 10), new SKPaint { Color = SKColors.Black });

            canvas.DrawText("Time", new SKPoint(w / 2, h), new SKPaint { Color = SKColors.Black });
            canvas.DrawText("Now", new SKPoint(20, h), new SKPaint { Color = SKColors.Black });
            canvas.DrawText("Previously", new SKPoint(w - 60, h), new SKPaint { Color = SKColors.Black });

            canvas.DrawText($"PEEP: {((int)(status.Peep * 10)) / 10000f}", new SKPoint(w / 2, 15), new SKPaint { Color = SKColors.Black });

            MaxPress = Math.Max(1, (MaxPress + thisMax) / 2);
            MinPress = Math.Min((MinPress + thisMin) / 2, MaxPress - 500);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;
            AnimationLoop();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }
    }
}