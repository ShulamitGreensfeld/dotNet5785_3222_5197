using System.Collections;
using System.Collections.Generic;

namespace PL;
internal class CallTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CallType> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.CallType)) as IEnumerable<BO.Enums.CallType>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}
internal class RoleCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.Role> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.Role)) as IEnumerable<BO.Enums.Role>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class DistanceTypesCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.DistanceTypes> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as IEnumerable<BO.Enums.DistanceTypes>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}