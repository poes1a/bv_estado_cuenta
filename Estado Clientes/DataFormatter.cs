using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Estado_Clientes
{
    public static class DataFormatter
    {
        public static int CheckForZero(this int value)
        {
            return value == 0 ? 1 : value;
        }
    }
}
