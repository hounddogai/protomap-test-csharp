namespace Acme.Workforce.Workplace.Desk.Profile;

using System.Globalization;

/// <summary>How screens and exports show a desk rollup.</summary>
public sealed record DeskRollupView(string Code, string Name, string Rate, string Skill, string BatchSizeText, string StatusLabel)
{
    public static DeskRollupView ToDeskRollupView(DeskRollup item)
    {
        return new DeskRollupView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskRollupView> ToDeskRollupViews(List<DeskRollup> items)
    {
        var views = new List<DeskRollupView>();
        foreach (var item in items)
        {
            views.Add(ToDeskRollupView(item));
        }
        return views;
    }

    public static double SumDeskRollupView(DeskRollupView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedBatchSize;
    }
}
