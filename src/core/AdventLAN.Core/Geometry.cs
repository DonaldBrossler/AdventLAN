using System;

namespace AdventLAN.Core
{
    public struct Vec2
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float s) => new Vec2(a.X * s, a.Y * s);

        public float LengthSquared() => X * X + Y * Y;
        public float Length() => MathF.Sqrt(LengthSquared());
    }

    /// <summary>Achsparalleles Rechteck (Hindernis).</summary>
    public struct Rect
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        public Rect(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }
    }

    /// <summary>Geometrie-Hilfen: Kreis gegen Rechteck, Strecke gegen Rechteck und Kreis.</summary>
    public static class Geo
    {
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);

        /// <summary>Abstand eines Punktes zum Rechteck (0, wenn der Punkt darin liegt).</summary>
        public static float DistanceToRect(Vec2 p, Rect r)
        {
            float dx = p.X - Clamp(p.X, r.MinX, r.MaxX);
            float dy = p.Y - Clamp(p.Y, r.MinY, r.MaxY);
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Schiebt einen Kreis aus einem Rechteck heraus. Gibt true zurück, wenn er verschoben wurde.</summary>
        public static bool ResolveCircleRect(ref Vec2 c, float radius, Rect r)
        {
            float cx = Clamp(c.X, r.MinX, r.MaxX);
            float cy = Clamp(c.Y, r.MinY, r.MaxY);
            float dx = c.X - cx;
            float dy = c.Y - cy;
            float d2 = dx * dx + dy * dy;
            if (d2 >= radius * radius) return false;

            if (d2 > 1e-12f)
            {
                float d = MathF.Sqrt(d2);
                float push = (radius - d) / d;
                c.X += dx * push;
                c.Y += dy * push;
            }
            else
            {
                // Mittelpunkt liegt im Rechteck: entlang der kleinsten Überlappung hinausschieben.
                float left = c.X - r.MinX;
                float right = r.MaxX - c.X;
                float top = c.Y - r.MinY;
                float bottom = r.MaxY - c.Y;
                float m = MathF.Min(MathF.Min(left, right), MathF.Min(top, bottom));
                if (m == left) c.X = r.MinX - radius;
                else if (m == right) c.X = r.MaxX + radius;
                else if (m == top) c.Y = r.MinY - radius;
                else c.Y = r.MaxY + radius;
            }
            return true;
        }

        /// <summary>Erster Treffpunkt der Strecke a→b mit dem Rechteck als Anteil t (0..1).</summary>
        public static bool SegmentRect(Vec2 a, Vec2 b, Rect r, out float t)
        {
            t = 0f;
            float tmin = 0f;
            float tmax = 1f;
            if (!Slab(a.X, b.X - a.X, r.MinX, r.MaxX, ref tmin, ref tmax)) return false;
            if (!Slab(a.Y, b.Y - a.Y, r.MinY, r.MaxY, ref tmin, ref tmax)) return false;
            t = tmin;
            return true;
        }

        private static bool Slab(float start, float delta, float lo, float hi, ref float tmin, ref float tmax)
        {
            if (MathF.Abs(delta) < 1e-9f) return start >= lo && start <= hi;
            float t1 = (lo - start) / delta;
            float t2 = (hi - start) / delta;
            if (t1 > t2)
            {
                float tmp = t1;
                t1 = t2;
                t2 = tmp;
            }
            if (t1 > tmin) tmin = t1;
            if (t2 < tmax) tmax = t2;
            return tmin <= tmax;
        }

        /// <summary>Erster Treffpunkt der Strecke a→b mit dem Kreis als Anteil t (0..1).</summary>
        public static bool SegmentCircle(Vec2 a, Vec2 b, Vec2 center, float radius, out float t)
        {
            t = 0f;
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float fx = a.X - center.X;
            float fy = a.Y - center.Y;
            float c = fx * fx + fy * fy - radius * radius;
            if (c <= 0f) return true; // Start liegt schon im Kreis

            float qa = dx * dx + dy * dy;
            if (qa < 1e-12f) return false;
            float qb = 2f * (fx * dx + fy * dy);
            float disc = qb * qb - 4f * qa * c;
            if (disc < 0f) return false;

            float t1 = (-qb - MathF.Sqrt(disc)) / (2f * qa);
            if (t1 < 0f || t1 > 1f) return false;
            t = t1;
            return true;
        }
    }
}
