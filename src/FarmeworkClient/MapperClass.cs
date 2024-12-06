using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmeworkClient
{
    [Mapper]
     public partial class MapperClass
    {
         public partial Dto ModelToDto(Model model);
    }
}
