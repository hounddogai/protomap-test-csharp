namespace Acme.Workforce.Talent.Mobility.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility snapshot.</summary>
public sealed record MobilitySnapshotView(string CodeText, string Name, string FteText, string Rate, string Unit, string Notes, string Revision, string Team, string CreatedAt, string TimezoneText, string JobCode, string StatusLabel)
{
    public static MobilitySnapshotView ToMobilitySnapshotView(MobilitySnapshot item)
    {
        return new MobilitySnapshotView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 64),
            item.Notes,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilitySnapshotView> ToMobilitySnapshotViews(List<MobilitySnapshot> items)
    {
        var views = new List<MobilitySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToMobilitySnapshotView(item));
        }
        return views;
    }

    public static double SumMobilitySnapshotView(MobilitySnapshotView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedFte + parsedRate + (double)parsedRevision;
    }
}
