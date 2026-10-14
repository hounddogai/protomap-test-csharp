namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using System.Globalization;

/// <summary>How screens and exports show a reorg entry.</summary>
public sealed record ReorgEntryView(string Code, string Name, string Currency, string Margin, string CostCenterText, string Unit, string Badge, string GraceDays, string ScoreText, string ManagerId, string StatusLabel)
{
    public static ReorgEntryView ToReorgEntryView(ReorgEntry item)
    {
        return new ReorgEntryView(
            item.Code,
            item.Name,
            item.Currency,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Unit,
            item.Badge,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgEntryView> ToReorgEntryViews(List<ReorgEntry> items)
    {
        var views = new List<ReorgEntryView>();
        foreach (var item in items)
        {
            views.Add(ToReorgEntryView(item));
        }
        return views;
    }

    public static double SumReorgEntryView(ReorgEntryView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedGraceDays + parsedScore;
    }
}
