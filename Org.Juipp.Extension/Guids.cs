// Guids.cs
// MUST match guids.h
using System;

namespace AdiswareLLC.Org_Juipp_Extension
{
    static class GuidList
    {
        public const string guidOrg_Juipp_ExtensionPkgString = "dabb4628-c8f9-4df1-9689-50b387a1b280";
        public const string guidOrg_Juipp_ExtensionCmdSetString = "9428683d-cb5a-4300-bd0e-c0d68d99309c";

        public static readonly Guid guidOrg_Juipp_ExtensionCmdSet = new Guid(guidOrg_Juipp_ExtensionCmdSetString);
    };
}