using System.Collections;
using System.Collections.Generic;

namespace PL;

/// <summary>
/// Collection class to enumerate over all CallType enum values.
/// </summary>
internal class CallTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CallType> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.CallType)) as IEnumerable<BO.Enums.CallType>)!;

    /// <summary>
    /// Returns an enumerator for the CallType enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Collection class to enumerate over all Role enum values.
/// </summary>
internal class RoleCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.Role> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.Role)) as IEnumerable<BO.Enums.Role>)!;

    /// <summary>
    /// Returns an enumerator for the Role enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Collection class to enumerate over all DistanceTypes enum values.
/// </summary>
internal class DistanceTypesCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.DistanceTypes> s_enums =
        (System.Enum.GetValues(typeof(BO.Enums.DistanceTypes)) as IEnumerable<BO.Enums.DistanceTypes>)!;

    /// <summary>
    /// Returns an enumerator for the DistanceTypes enum values.
    /// </summary>
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}