namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pay band quota.</summary>
public sealed record StrictPayBandQuotaView(string Code, string LabelText, string Ceiling, string BatchSize, string UpdatedAt, string SourceText, string SalaryBand, string StatusLabel)
{
    public static StrictPayBandQuotaView ToStrictPayBandQuotaView(StrictPayBandQuota item)
    {
        return new StrictPayBandQuotaView(
            item.Code,
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Source,
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPayBandQuotaView> ToStrictPayBandQuotaViews(List<StrictPayBandQuota> items)
    {
        var views = new List<StrictPayBandQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPayBandQuotaView(item));
        }
        return views;
    }

    public static double SumStrictPayBandQuotaView(StrictPayBandQuotaView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedBatchSize + (double)parsedUpdatedAt;
    }
}
