using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Domain.Common.Enums
{
    public enum PlatformEnvironment
    {
        Production = 1,
        Local = 2,
        LocalProductionDatabase = 3
    }

    public enum PlatformType
    {
        Web = 1,
        Console = 2,
        UnitTest = 3
    }
}
