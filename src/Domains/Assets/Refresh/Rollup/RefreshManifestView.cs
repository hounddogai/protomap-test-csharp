namespace Acme.Workforce.Assets.Refresh.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh manifest.</summary>
public sealed record RefreshManifestView(string CodeText, string Label, string JobCodeText, string Active, string LeaveDays, string HoursText, string Ceiling, string Skill, string Reviewer, string StatusLabel)
{
    public static RefreshManifestView ToRefreshManifestView(RefreshManifest item)
    {
        return new RefreshManifestView(
            item.Code,
            item.Label,
            Text.Truncate(item.JobCode, 64),
            $"{item.Active}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshManifestView> ToRefreshManifestViews(List<RefreshManifest> items)
    {
        var views = new List<RefreshManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshManifestView(item));
        }
        return views;
    }

    public static double SumRefreshManifestView(RefreshManifestView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedHours + parsedCeiling;
    }
}
