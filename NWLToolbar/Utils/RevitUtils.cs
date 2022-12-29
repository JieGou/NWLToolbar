using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
    }
}
