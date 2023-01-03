using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace NWLToolbar.Utils
{
    public static class RevitUtils
    {
        public static string GetName(this WallType wt)
        {
            return $"{wt.FamilyName}: {wt.Name}";
        }
        public static string GetName(this ViewFamilyType vft)
        {
            return $"{vft.FamilyName}: {vft.Name}";
        }
        public static string GetName(this Room r)
        {
            return r.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString().ToString();
        }
        public static Parameter GetNameParam(this Room r)
        {
            return r.get_Parameter(BuiltInParameter.ROOM_NAME);
        }
        public static string GetName(this FamilySymbol fs)
        {
            return $"{fs.FamilyName}: {fs.Name}";
        }
        public static string GetNumName(this Room r)
        {
            return $"{r.Number} - {r.GetName()}";
        }
        public static string GetDetailNumber(this Viewport v)
        {
            return v.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).AsValueString();
        }
        public static double GetHeight(this Ceiling clg)
        {
            return clg.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
        }
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
        public static bool IsEnclosed(this Room room)
        {
            var isEnclosed = room.Area > 0;

            return isEnclosed;
        }
        public static double GetViewDepth(IList<BoundarySegment> roomBoundry, double v1, XYZ roomCenter)
        {
            if (v1 == 0)
            {
                double depth = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).X);
                return Math.Abs(roomCenter.X - depth);
            }
            if (v1 == 1)
            {
                double depth = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).Y);
                return Math.Abs(depth - roomCenter.Y);
            }
            else if (v1 == 2)
            {
                double depth = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).X);
                return Math.Abs(roomCenter.X - depth);
            }
            //(v1 == 3)
            {
                double depth = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).Y);
                return Math.Abs(depth - roomCenter.Y);
            }
        }
        public static XYZ GetRoomMax(IList<BoundarySegment> roomBoundry, XYZ direction, double ceilingHeight)
        {
            double max = 0;
            if (Math.Abs(direction.X) == 1)
            {
                if(direction.X < 0)
                    max = -roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).Y);
                else
                    max = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).Y);

                return new XYZ(max, ceilingHeight, 0);
            }
            if (Math.Abs(direction.Y) == 1)
            {
                if (direction.Y < 0)
                    max = roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).X);
                else
                    max = -roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).X);

                return new XYZ(max, ceilingHeight, 0);
            }
            else
            {
                return null;
            }                    
        }
        public static XYZ GetRoomMin(IList<BoundarySegment> roomBoundry, XYZ direction, double roomLevel)
        {
            double min = 0;
            if (Math.Abs(direction.X) == 1)
            {
                if (direction.X < 0)
                    min = -roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).Y);
                else
                    min = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).Y);

                return new XYZ(min, roomLevel, 0);
            }
            if (Math.Abs(direction.Y) == 1)
            {
                if (direction.Y < 0)
                    min = roomBoundry.Min(x => x.GetCurve().GetEndPoint(0).X);
                else
                    min = -roomBoundry.Max(x => x.GetCurve().GetEndPoint(0).X);

                return new XYZ(min, roomLevel, 0);
            }
            else
            {
                return null;
            }
        }
    }
}
