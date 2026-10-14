namespace Acme.Workforce.Compliance.Exception.Audit;

using System.Globalization;

/// <summary>How screens and exports show a default exception quota.</summary>
public sealed record DefaultExceptionQuotaView(string Code, string TitleText, string TenureMonths, string Cost, string OwnerId, string Rating, string MarginText, string UpdatedAt, string CountryText, string StatusLabel)
{
    public static DefaultExceptionQuotaView ToDefaultExceptionQuotaView(DefaultExceptionQuota item)
    {
        return new DefaultExceptionQuotaView(
            item.Code,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultExceptionQuotaView> ToDefaultExceptionQuotaViews(List<DefaultExceptionQuota> items)
    {
        var views = new List<DefaultExceptionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultExceptionQuotaView(item));
        }
        return views;
    }

    public static double SumDefaultExceptionQuotaView(DefaultExceptionQuotaView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedCost + parsedRating;
    }
}
