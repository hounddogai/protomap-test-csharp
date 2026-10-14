namespace Acme.Workforce.Assets.Warranty.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary warranty profile.</summary>
public sealed record PrimaryWarrantyProfileView(string Code, string LabelText, string LeaveDays, string Channel, string BatchSize, string Currency, string Rating, string Ratio, string StatusLabel)
{
    public static PrimaryWarrantyProfileView ToPrimaryWarrantyProfileView(PrimaryWarrantyProfile item)
    {
        return new PrimaryWarrantyProfileView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryWarrantyProfileView> ToPrimaryWarrantyProfileViews(List<PrimaryWarrantyProfile> items)
    {
        var views = new List<PrimaryWarrantyProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryWarrantyProfileView(item));
        }
        return views;
    }

    public static double SumPrimaryWarrantyProfileView(PrimaryWarrantyProfileView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedBatchSize + parsedRating;
    }
}
