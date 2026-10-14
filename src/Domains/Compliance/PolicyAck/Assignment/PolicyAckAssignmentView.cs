namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a policy ack assignment.</summary>
public sealed record PolicyAckAssignmentView(string Code, string Title, string AttemptsText, string Minimum, string DurationMinutes, string RatioText, string Pinned, string GraceDays, string Locale, string StatusLabel)
{
    public static PolicyAckAssignmentView ToPolicyAckAssignmentView(PolicyAckAssignment item)
    {
        return new PolicyAckAssignmentView(
            item.Code,
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckAssignmentView> ToPolicyAckAssignmentViews(List<PolicyAckAssignment> items)
    {
        var views = new List<PolicyAckAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckAssignmentView(item));
        }
        return views;
    }

    public static double SumPolicyAckAssignmentView(PolicyAckAssignmentView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedMinimum + (double)parsedDurationMinutes;
    }
}
