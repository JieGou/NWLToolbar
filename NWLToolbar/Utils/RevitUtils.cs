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
        public static double GetHeight(this Ceiling clg)
        {
            return clg.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
        }
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}
