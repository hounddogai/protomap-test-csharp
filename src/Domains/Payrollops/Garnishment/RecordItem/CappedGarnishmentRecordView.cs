namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped garnishment record.</summary>
public sealed record CappedGarnishmentRecordView(string Code, string Name, string Limit, string StartsAt, string Weight, string CurrencyText, string Approved, string RetriesText, string StatusLabel)
{
    public static CappedGarnishmentRecordView ToCappedGarnishmentRecordView(CappedGarnishmentRecord item)
    {
        return new CappedGarnishmentRecordView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Approved}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedGarnishmentRecordView> ToCappedGarnishmentRecordViews(List<CappedGarnishmentRecord> items)
    {
        var views = new List<CappedGarnishmentRecordView>();
        foreach (var item in items)
        {
            views.Add(ToCappedGarnishmentRecordView(item));
        }
        return views;
    }

    public static double SumCappedGarnishmentRecordView(CappedGarnishmentRecordView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedStartsAt + parsedWeight;
    }
}
