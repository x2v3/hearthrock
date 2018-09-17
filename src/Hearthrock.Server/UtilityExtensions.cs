using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthrock.Server
{
    public static class UtilityExtensions
    {
        public static string ToItemsString(this IEnumerable<int> items)
        {
            return items==null?"": string.Join(",", items);
        }
    }
}
