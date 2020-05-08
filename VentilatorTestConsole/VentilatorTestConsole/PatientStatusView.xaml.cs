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
            MaxPress = 1;
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
            float startY = startVal / MaxVol;
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

                float endY = endVal / MaxVol;
                endY = h - (endY * h);

                canvas.DrawLine(startX, startY, endX, endY, VolPaint);

                startX = endX;
                startY = endY;
            }

            MaxVol = Math.Max(0.25F, (MaxVol + thisMax) / 2);
        }

        async Task AnimationLoop()
        {
            stopwatch.Start();

            while (pageIsActive)
            {
                VolumeDisplay.InvalidateSurface();
                PressureDisplay.InvalidateSurface();
                await Task.Delay(TimeSpan.FromSeconds(1.0 / 30));
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

            float startVal = status.RecentPressMeasurements.Get(0);
            if (startVal > thisMax)
            {
                thisMax = startVal;
            }
            float startY = startVal / MaxPress;
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

                float endY = endVal / MaxPress;
                endY = h - (endY * h);

                canvas.DrawLine(startX, startY, endX, endY, VolPaint);

                startX = endX;
                startY = endY;
            }

            MaxPress = Math.Max(1, (MaxVol + thisMax) / 2);
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