namespace RiderMaui.Service;
using SkiaSharp;

public struct Vec3
{
    public double X, Y, Z;
    public Vec3(double x, double y, double z) => (X, Y, Z) = (x, y, z);
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 a, double t) => new(a.X * t, a.Y * t, a.Z * t);
    public static Vec3 operator *(double t, Vec3 a) => a * t;
    public static Vec3 operator /(Vec3 a, double t) => new(a.X / t, a.Y / t, a.Z / t);
    public static Vec3 operator -(Vec3 a) => new(-a.X, -a.Y, -a.Z);
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);
    public Vec3 Normalize() => this * (1.0 / Length);
    public double Dot(Vec3 other) => X * other.X + Y * other.Y + Z * other.Z;
}

public class Ray
{
    public Vec3 Origin { get; }
    public Vec3 Direction { get; }
    public Ray(Vec3 origin, Vec3 direction) => (Origin, Direction) = (origin, direction.Normalize());
    public Vec3 At(double t) => Origin + Direction * t;
    public static Vec3 Reflect(Vec3 v, Vec3 n) => v - n * (2 * v.Dot(n));
}

public class Sphere
{
    public Vec3 Center { get; }
    public double Radius { get; }
    public SKColor Color { get; } // Используем SKColor напрямую

    public Sphere(Vec3 center, double radius, SKColor color)
        => (Center, Radius, Color) = (center, radius, color);

    public bool Hit(Ray ray, double tMin, double tMax, out HitRecord record)
    {
        record = default;
        var oc = ray.Origin - Center;
        var a = ray.Direction.Dot(ray.Direction);
        var b = 2.0 * oc.Dot(ray.Direction);
        var c = oc.Dot(oc) - Radius * Radius;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0) return false;
        var sqrtd = Math.Sqrt(discriminant);
        var root = (-b - sqrtd) / (2.0 * a);
        if (root < tMin || root > tMax)
        {
            root = (-b + sqrtd) / (2.0 * a);
            if (root < tMin || root > tMax) return false;
        }

        record.T = root;
        record.Point = ray.At(root);
        var outwardNormal = (record.Point - Center) / Radius;
        record.SetFaceNormal(ray, outwardNormal);
        return true;
    }
}

public struct HitRecord
{
    public Vec3 Point;
    public Vec3 Normal;
    public double T;
    public bool FrontFace;
    public void SetFaceNormal(Ray ray, Vec3 outwardNormal)
    {
        FrontFace = ray.Direction.Dot(outwardNormal) < 0;
        Normal = FrontFace ? outwardNormal : -outwardNormal;
    }
}

public class Scene
{
    public List<Sphere> Objects { get; } = new();
    public List<Light> Lights { get; } = new();
    public Vec3 Camera { get; set; } = new(0, 0, 0);
    public double AspectRatio { get; set; } = 16.0 / 9.0;
    public double ViewportHeight { get; set; } = 2.0;
    public double FocalLength { get; set; } = 1.0;
}

public class Light
{
    public Vec3 Position { get; }
    public double Brightness { get; }
    public Light(Vec3 position, double brightness = 1.0)
        => (Position, Brightness) = (position, brightness);
}