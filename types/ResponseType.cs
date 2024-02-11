using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerToys_Run_Huh.types;
internal enum ResponseType
{
    Question,
    Answer,
    Error
}
internal record ContextData(ResponseType ResponseType, string? Message = null)
{
}
