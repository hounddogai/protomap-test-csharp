namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a seasonal span profile.</summary>
public sealed record SeasonalSpanProfileView(string Code, string NameText, string Maximum, string Amount, string Attempts, string StatusLabel)
{
    public static SeasonalSpanProfileView ToSeasonalSpanProfileView(SeasonalSpanProfile item)
    {
        return new SeasonalSpanProfileView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSpanProfileView> ToSeasonalSpanProfileViews(List<SeasonalSpanProfile> items)
    {
        var views = new List<SeasonalSpanProfileView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSpanProfileView(item));
        }
        return views;
    }

    public static double SumSeasonalSpanProfileView(SeasonalSpanProfileView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedAmount + (double)parsedAttempts;
    }
}
