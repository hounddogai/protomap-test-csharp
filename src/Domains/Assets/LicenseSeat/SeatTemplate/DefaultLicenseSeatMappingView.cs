namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using System.Globalization;

/// <summary>How screens and exports show a default license seat mapping.</summary>
public sealed record DefaultLicenseSeatMappingView(string Code, string Title, string RateText, string BatchSizeText, string QuantityText, string PriorityText, string FloorValue, string Revision, string Weight, string Score, string StatusLabel)
{
    public static DefaultLicenseSeatMappingView ToDefaultLicenseSeatMappingView(DefaultLicenseSeatMapping item)
    {
        return new DefaultLicenseSeatMappingView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLicenseSeatMappingView> ToDefaultLicenseSeatMappingViews(List<DefaultLicenseSeatMapping> items)
    {
        var views = new List<DefaultLicenseSeatMappingView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLicenseSeatMappingView(item));
        }
        return views;
    }

    public static double SumDefaultLicenseSeatMappingView(DefaultLicenseSeatMappingView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedBatchSize + (double)parsedQuantity;
    }
}
