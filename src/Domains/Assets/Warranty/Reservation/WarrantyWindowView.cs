namespace Acme.Workforce.Assets.Warranty.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty window.</summary>
public sealed record WarrantyWindowView(string Code, string Label, string Channel, string BatchSizeText, string LimitText, string Threshold, string Retries, string JobCode, string StatusLabel)
{
    public static WarrantyWindowView ToWarrantyWindowView(WarrantyWindow item)
    {
        return new WarrantyWindowView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Channel,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyWindowView> ToWarrantyWindowViews(List<WarrantyWindow> items)
    {
        var views = new List<WarrantyWindowView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyWindowView(item));
        }
        return views;
    }

    public static double SumWarrantyWindowView(WarrantyWindowView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedLimit + parsedThreshold;
    }
}
