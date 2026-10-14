namespace Acme.Workforce.Talent.Nomination.Group;

using System.Globalization;

/// <summary>How screens and exports show a nomination assignment.</summary>
public sealed record NominationAssignmentView(string Code, string Label, string HoursText, string Channel, string LimitText, string Score, string Approved, string ChecksumText, string StatusLabel)
{
    public static NominationAssignmentView ToNominationAssignmentView(NominationAssignment item)
    {
        return new NominationAssignmentView(
            item.Code,
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationAssignmentView> ToNominationAssignmentViews(List<NominationAssignment> items)
    {
        var views = new List<NominationAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToNominationAssignmentView(item));
        }
        return views;
    }

    public static double SumNominationAssignmentView(NominationAssignmentView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedLimit + parsedScore;
    }
}
