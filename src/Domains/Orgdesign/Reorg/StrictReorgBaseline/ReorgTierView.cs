namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using System.Globalization;

/// <summary>How screens and exports show a reorg tier.</summary>
public sealed record ReorgTierView(string CodeText, string TitleText, string UpdatedAt, string CreatedAtText, string Active, string WindowDays, string BatchSize, string EndsAt, string Category, string Checksum, string StatusLabel)
{
    public static ReorgTierView ToReorgTierView(ReorgTier item)
    {
        return new ReorgTierView(
            item.Code,
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgTierView> ToReorgTierViews(List<ReorgTier> items)
    {
        var views = new List<ReorgTierView>();
        foreach (var item in items)
        {
            views.Add(ToReorgTierView(item));
        }
        return views;
    }

    public static double SumReorgTierView(ReorgTierView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedCreatedAt + (double)parsedWindowDays;
    }
}
