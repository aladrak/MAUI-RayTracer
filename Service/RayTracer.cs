using SkiaSharp;
namespace RiderMaui.Service;

public static class RayTracer
{
    public static SKBitmap Render(Scene scene, int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        var viewportWidth = scene.ViewportHeight * scene.AspectRatio;
        var origin = scene.Camera;
        var horizontal = new Vec3(viewportWidth, 0, 0);
        var vertical = new Vec3(0, -scene.ViewportHeight, 0);
        var lowerLeftCorner = origin - horizontal / 2 - vertical / 2 - new Vec3(0, 0, scene.FocalLength);

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                var u = (double)i / (width - 1);
                var v = (double)j / (height - 1);
                var ray = new Ray(origin, (lowerLeftCorner + u * horizontal + v * vertical - origin).Normalize());
                var color = RayColor(ray, scene);
                bitmap.SetPixel(i, j, color);
            }
        }
        return bitmap;
    }

    static SKColor RayColor(Ray ray, Scene scene)
    {
        const double tMin = 0.001;
        const double tMax = 100.0;

        HitRecord closestHit = default;
        Sphere closestSphere = null;
        double closestT = tMax;

        foreach (var sphere in scene.Objects)
        {
            if (sphere.Hit(ray, tMin, closestT, out var hit) && hit.T < closestT)
            {
                closestT = hit.T;
                closestHit = hit;
                closestSphere = sphere;
            }
        }

        if (closestSphere != null)
        {
            var color = closestSphere.Color;
            var totalLight = 0.1; // ambient

            foreach (var light in scene.Lights)
            {
                var lightDir = light.Position - closestHit.Point;
                var lightDistance = lightDir.Length;
                var shadowRay = new Ray(closestHit.Point, lightDir.Normalize());

                bool inShadow = false;
                foreach (var obj in scene.Objects)
                {
                    if (obj.Hit(shadowRay, tMin, lightDistance - tMin, out _))
                    {
                        inShadow = true;
                        break;
                    }
                }

                if (!inShadow)
                {
                    var diffuse = Math.Max(0, closestHit.Normal.Dot(shadowRay.Direction));
                    totalLight += diffuse * light.Brightness;
                }
            }

            totalLight = Math.Min(totalLight, 1.0);
            return new SKColor(
                (byte)(color.Red * totalLight),
                (byte)(color.Green * totalLight),
                (byte)(color.Blue * totalLight)
            );
        }

        // Фон
        var unitDir = ray.Direction.Normalize();
        var t = 0.5 * (unitDir.Y + 1.0);
        return new SKColor(
            (byte)(255 * (1.0 - t * 0.5)),
            (byte)(255 * (1.0 - t * 0.5)),
            (byte)(255 * (1.0 - t * 0.5 + t))
        );
    }
}