namespace Acme.Workforce.Payrollops.Deduction.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict deduction quota.</summary>
public sealed record StrictDeductionQuotaView(string CodeText, string Label, string CeilingText, string Retries, string CurrencyText, string StatusLabel)
{
    public static StrictDeductionQuotaView ToStrictDeductionQuotaView(StrictDeductionQuota item)
    {
        return new StrictDeductionQuotaView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictDeductionQuotaView> ToStrictDeductionQuotaViews(List<StrictDeductionQuota> items)
    {
        var views = new List<StrictDeductionQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToStrictDeductionQuotaView(item));
        }
        return views;
    }

    public static double SumStrictDeductionQuotaView(StrictDeductionQuotaView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedRetries;
    }
}
