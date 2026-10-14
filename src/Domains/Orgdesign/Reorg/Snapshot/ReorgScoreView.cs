namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a reorg score.</summary>
public sealed record ReorgScoreView(string Code, string LabelText, string FloorValueText, string Version, string Channel, string Fte, string StatusLabel)
{
    public static ReorgScoreView ToReorgScoreView(ReorgScore item)
    {
        return new ReorgScoreView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgScoreView> ToReorgScoreViews(List<ReorgScore> items)
    {
        var views = new List<ReorgScoreView>();
        foreach (var item in items)
        {
            views.Add(ToReorgScoreView(item));
        }
        return views;
    }

    public static double SumReorgScoreView(ReorgScoreView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedVersion + parsedFte;
    }
}
