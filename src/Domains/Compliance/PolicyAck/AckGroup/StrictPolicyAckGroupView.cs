namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using System.Globalization;

/// <summary>How screens and exports show a strict policy ack group.</summary>
public sealed record StrictPolicyAckGroupView(string Code, string Name, string Department, string UnitText, string ExpiresAt, string Sequence, string StartsAt, string StatusLabel)
{
    public static StrictPolicyAckGroupView ToStrictPolicyAckGroupView(StrictPolicyAckGroup item)
    {
        return new StrictPolicyAckGroupView(
            item.Code,
            item.Name,
            item.Department,
            item.Unit,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyAckGroupView> ToStrictPolicyAckGroupViews(List<StrictPolicyAckGroup> items)
    {
        var views = new List<StrictPolicyAckGroupView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyAckGroupView(item));
        }
        return views;
    }

    public static double SumStrictPolicyAckGroupView(StrictPolicyAckGroupView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedSequence + (double)parsedStartsAt;
    }
}
