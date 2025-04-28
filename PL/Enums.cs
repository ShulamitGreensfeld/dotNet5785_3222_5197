using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL;
internal class CallTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CallType> s_enums =
(Enum.GetValues(typeof(BO.Enums.CallType)) as IEnumerable<BO.Enums.CallType>)!;

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}