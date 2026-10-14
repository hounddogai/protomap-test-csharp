namespace Acme.Workforce.Contractors.Rate.Budget;

using System.Globalization;

/// <summary>How screens and exports show a rate group.</summary>
public sealed record RateGroupView(string Code, string Label, string Retries, string Fte, string Region, string Margin, string CurrencyText, string Summary, string StatusLabel)
{
    public static RateGroupView ToRateGroupView(RateGroup item)
    {
        return new RateGroupView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateGroupView> ToRateGroupViews(List<RateGroup> items)
    {
        var views = new List<RateGroupView>();
        foreach (var item in items)
        {
            views.Add(ToRateGroupView(item));
        }
        return views;
    }

    public static double SumRateGroupView(RateGroupView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedFte + parsedMargin;
    }
}
