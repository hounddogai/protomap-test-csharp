namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request tier.</summary>
public sealed record LeaveRequestTierView(string CodeText, string LabelText, string Maximum, string Grade, string Version, string Weight, string StatusLabel)
{
    public static LeaveRequestTierView ToLeaveRequestTierView(LeaveRequestTier item)
    {
        return new LeaveRequestTierView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestTierView> ToLeaveRequestTierViews(List<LeaveRequestTier> items)
    {
        var views = new List<LeaveRequestTierView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestTierView(item));
        }
        return views;
    }

    public static double SumLeaveRequestTierView(LeaveRequestTierView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedVersion + parsedWeight;
    }
}
