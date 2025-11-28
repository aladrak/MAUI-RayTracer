using RiderMaui.Service;

namespace RiderMaui;

using SkiaSharp;
using SkiaSharp.Views.Maui;

public partial class MainPage : ContentPage
{
    private SKBitmap? _renderedBitmap;  
    public MainPage()
    {
        InitializeComponent();
        Task.Run(() => RenderScene());
    }
    
    async void RenderScene()
    {
        var scene = new Scene
        {
            Camera = new Vec3(0, 0, 1.2),
            Objects =
            {
                new Sphere(new Vec3(0, 0.4, 0), 0.5, new SKColor(255, 100, 100)),
                new Sphere(new Vec3(0, -100.5, -1), 100, new SKColor(128, 128, 128))
            },
            Lights =
            {
                new Light(new Vec3(0, 5, -5), 0.8)
            }
        };

        _renderedBitmap = RayTracer.Render(scene, 1200, 600);
        await MainThread.InvokeOnMainThreadAsync(() => canvasView.InvalidateSurface());
    }

    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        if (_renderedBitmap != null)
            canvas.DrawBitmap(_renderedBitmap, new SKRect(0, 0, e.Info.Width, e.Info.Height));
    }
    async void OnRenderClicked(object sender, EventArgs e)
    {
        double camX, camY, camZ,
            sphX, sphY, sphZ, radius,
            lightX, lightY, lightZ, brightness;
        try
        {
            double.TryParse(CamXEntry.Text, out camX);
            double.TryParse(CamYEntry.Text, out camY);
            double.TryParse(CamZEntry.Text, out camZ);
            double.TryParse(SphereXEntry.Text, out sphX);
            double.TryParse(SphereYEntry.Text, out sphY);
            double.TryParse(SphereZEntry.Text, out sphZ);
            double.TryParse(SphereRadiusEntry.Text, out radius);
            double.TryParse(LightXEntry.Text, out lightX);
            double.TryParse(LightYEntry.Text, out lightY);
            double.TryParse(LightZEntry.Text, out lightZ);
            double.TryParse(BrightnessEntry.Text, out brightness);
        }
        catch
        {
            await DisplayAlert("Ошибка", "Введите корректные числовые значения.", "OK");
            return;
        }

        if (radius <= 0)
        {
            await DisplayAlert("Ошибка", "Радиус сферы должен быть положительным.", "OK");
            return;
        }

        var scene = new Scene
        {
            Camera = new Vec3(camX, camY, camZ),
            Objects =
            {
                new Sphere(new Vec3(sphX, sphY, sphZ), radius, new SKColor(255, 100, 100)),
                new Sphere(new Vec3(0, -100.5, -1), 100, new SKColor(128, 128, 128))
            },
            Lights =
            {
                new Light(new Vec3(lightX, lightY, lightZ), brightness)
            }
        };

        _renderedBitmap = null;
        canvasView.InvalidateSurface();
        
        await Task.Run(() => {
            _renderedBitmap = RayTracer.Render(scene, 1200, 600);
        });

        canvasView.InvalidateSurface();
    }

    void OnResetClicked(object sender, EventArgs e)
    {
        CamXEntry.Text = "0";
        CamYEntry.Text = "0";
        CamZEntry.Text = "0";
        SphereXEntry.Text = "0.0";
        SphereYEntry.Text = "0.0";
        SphereZEntry.Text = "-1";
        SphereRadiusEntry.Text = "0.5";
        LightXEntry.Text = "5";
        LightYEntry.Text = "5";
        LightZEntry.Text = "-5";
        BrightnessEntry.Text = "0.8";
        _renderedBitmap.Reset();
        canvasView.InvalidateSurface();
    }

    async void OnDownloadClicked(object sender, EventArgs e)
    {
        using (var image = SKImage.FromBitmap(_renderedBitmap))
        using (var encodedData = image.Encode(SKEncodedImageFormat.Png, 100))
        using
            (var stream = File.OpenWrite(
                 Path.Combine(
                     Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                     "RayImage.png"
                 )
             )
            )
        {
            encodedData.SaveTo(stream);
        }
        await DisplayAlert("Image saved!", $"The image is saved to the path: {Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}", "OK");
    }
}