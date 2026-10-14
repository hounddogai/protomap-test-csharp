namespace Acme.Workforce.Assets.Warranty.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional warranty exception.</summary>
public sealed record RegionalWarrantyExceptionView(string Code, string Name, string CostText, string Amount, string Currency, string Rating, string MarginText, string CeilingText, string RevisionText, string RetriesText, string StatusLabel)
{
    public static RegionalWarrantyExceptionView ToRegionalWarrantyExceptionView(RegionalWarrantyException item)
    {
        return new RegionalWarrantyExceptionView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalWarrantyExceptionView> ToRegionalWarrantyExceptionViews(List<RegionalWarrantyException> items)
    {
        var views = new List<RegionalWarrantyExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalWarrantyExceptionView(item));
        }
        return views;
    }

    public static double SumRegionalWarrantyExceptionView(RegionalWarrantyExceptionView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedCost + parsedAmount + parsedRating;
    }
}
