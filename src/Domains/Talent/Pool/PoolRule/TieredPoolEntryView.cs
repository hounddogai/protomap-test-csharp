namespace Acme.Workforce.Talent.Pool.PoolRule;

using System.Globalization;

/// <summary>How screens and exports show a tiered pool entry.</summary>
public sealed record TieredPoolEntryView(string Code, string NameText, string Category, string Maximum, string Notes, string Budget, string Location, string Currency, string CountryText, string StatusLabel)
{
    public static TieredPoolEntryView ToTieredPoolEntryView(TieredPoolEntry item)
    {
        return new TieredPoolEntryView(
            item.Code,
            item.Name,
            item.Category,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Currency,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPoolEntryView> ToTieredPoolEntryViews(List<TieredPoolEntry> items)
    {
        var views = new List<TieredPoolEntryView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPoolEntryView(item));
        }
        return views;
    }

    public static double SumTieredPoolEntryView(TieredPoolEntryView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedBudget;
    }
}
