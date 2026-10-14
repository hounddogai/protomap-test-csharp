namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback garnishment record.</summary>
public sealed record FallbackGarnishmentRecordView(string Code, string TitleText, string OwnerId, string Country, string ManagerId, string Currency, string FteText, string Channel, string Weight, string Margin, string StatusLabel)
{
    public static FallbackGarnishmentRecordView ToFallbackGarnishmentRecordView(FallbackGarnishmentRecord item)
    {
        return new FallbackGarnishmentRecordView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.OwnerId, 64),
            item.Country,
            item.ManagerId,
            Text.Truncate(item.Currency, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackGarnishmentRecordView> ToFallbackGarnishmentRecordViews(List<FallbackGarnishmentRecord> items)
    {
        var views = new List<FallbackGarnishmentRecordView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackGarnishmentRecordView(item));
        }
        return views;
    }

    public static double SumFallbackGarnishmentRecordView(FallbackGarnishmentRecordView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedFte + parsedWeight + parsedMargin;
    }
}
