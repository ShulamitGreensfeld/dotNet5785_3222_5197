using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PL
{
    internal class CallTypeCollection : IEnumerable
    {
        static readonly IEnumerable<string> s_enums =
            Enum.GetValues(typeof(BO.Enums.CallType))
                .Cast<BO.Enums.CallType>()
                .Select(e => e.ToString());

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}
