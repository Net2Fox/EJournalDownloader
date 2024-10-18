using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EJournalWPF.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum Status
    {
        all,
        unread,
        read
    }
}
