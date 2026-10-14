namespace Acme.Workforce.Absence.LeaveRequest.Review;

using System.Globalization;

/// <summary>How screens and exports show a leave request rollup.</summary>
public sealed record LeaveRequestRollupView(string Code, string Label, string Fte, string JobCode, string Version, string StatusLabel)
{
    public static LeaveRequestRollupView ToLeaveRequestRollupView(LeaveRequestRollup item)
    {
        return new LeaveRequestRollupView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestRollupView> ToLeaveRequestRollupViews(List<LeaveRequestRollup> items)
    {
        var views = new List<LeaveRequestRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestRollupView(item));
        }
        return views;
    }

    public static double SumLeaveRequestRollupView(LeaveRequestRollupView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedVersion;
    }
}
