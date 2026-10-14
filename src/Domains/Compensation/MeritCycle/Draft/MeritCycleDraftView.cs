namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle draft.</summary>
public sealed record MeritCycleDraftView(string Code, string NameText, string Title, string Visible, string LocationText, string Team, string VersionText, string RegionText, string StatusLabel)
{
    public static MeritCycleDraftView ToMeritCycleDraftView(MeritCycleDraft item)
    {
        return new MeritCycleDraftView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Title,
            $"{item.Visible}",
            Text.Truncate(item.Location, 120),
            item.Team,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleDraftView> ToMeritCycleDraftViews(List<MeritCycleDraft> items)
    {
        var views = new List<MeritCycleDraftView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleDraftView(item));
        }
        return views;
    }

    public static double SumMeritCycleDraftView(MeritCycleDraftView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
