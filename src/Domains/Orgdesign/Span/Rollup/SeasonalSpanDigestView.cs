namespace Acme.Workforce.Orgdesign.Span.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a seasonal span digest.</summary>
public sealed record SeasonalSpanDigestView(string Code, string Name, string Notes, string Visible, string Ceiling, string ApprovedText, string Source, string OwnerId, string Amount, string LocationText, string Description, string StatusLabel)
{
    public static SeasonalSpanDigestView ToSeasonalSpanDigestView(SeasonalSpanDigest item)
    {
        return new SeasonalSpanDigestView(
            item.Code,
            item.Name,
            item.Notes,
            $"{item.Visible}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Source,
            item.OwnerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalSpanDigestView> ToSeasonalSpanDigestViews(List<SeasonalSpanDigest> items)
    {
        var views = new List<SeasonalSpanDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalSpanDigestView(item));
        }
        return views;
    }

    public static double SumSeasonalSpanDigestView(SeasonalSpanDigestView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedAmount;
    }
}
