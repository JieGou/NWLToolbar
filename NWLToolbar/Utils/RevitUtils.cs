using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

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
    }
}
