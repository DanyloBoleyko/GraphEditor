using System.Numerics;
using Windows.Foundation;

namespace GraphEditorWPF.Types
{
    public class Coordinates
    {
        private double _x = 0, _y = 0;

        public Coordinates(double x = 0, double y = 0)
        {
            _x = x;
            _y = y;
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public static explicit operator Coordinates(Point point)
        {
            return new Coordinates(point.X, point.Y);
        }

        public static explicit operator Coordinates(Vector2 vector)
        {
            return new Coordinates(vector.X, vector.Y);
        }

        public static bool operator ==(Coordinates obj1, Coordinates obj2)
        {
            if (obj1.X == obj2.X && obj1.Y == obj2.Y) return true;
            return false;
        }
        public static bool operator !=(Coordinates obj1, Coordinates obj2) => !(obj1 == obj2);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Coordinates))
            {
                return false;
            }

            var item = obj as Coordinates;

            return X == item.X && Y == item.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                hash += X.GetHashCode();
                hash += Y.GetHashCode();
                return hash;
            }
        }
    }
}
