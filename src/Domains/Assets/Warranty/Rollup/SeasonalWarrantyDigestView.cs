namespace Acme.Workforce.Assets.Warranty.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal warranty digest.</summary>
public sealed record SeasonalWarrantyDigestView(string Code, string Title, string JobCode, string Visible, string Team, string CurrencyText, string Amount, string Retries, string Revision, string StatusLabel)
{
    public static SeasonalWarrantyDigestView ToSeasonalWarrantyDigestView(SeasonalWarrantyDigest item)
    {
        return new SeasonalWarrantyDigestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.JobCode, 80),
            $"{item.Visible}",
            item.Team,
            Text.Truncate(item.Currency, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalWarrantyDigestView> ToSeasonalWarrantyDigestViews(List<SeasonalWarrantyDigest> items)
    {
        var views = new List<SeasonalWarrantyDigestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalWarrantyDigestView(item));
        }
        return views;
    }

    public static double SumSeasonalWarrantyDigestView(SeasonalWarrantyDigestView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedRetries + (double)parsedRevision;
    }
}
