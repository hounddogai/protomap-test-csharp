namespace Acme.Workforce.Compensation.PayBand.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pay band audit.</summary>
public sealed record StrictPayBandAuditView(string Code, string Label, string Archived, string Badge, string JobCodeText, string ChecksumText, string SourceText, string Amount, string StatusLabel)
{
    public static StrictPayBandAuditView ToStrictPayBandAuditView(StrictPayBandAudit item)
    {
        return new StrictPayBandAuditView(
            item.Code,
            Text.Truncate(item.Label, 40),
            $"{item.Archived}",
            item.Badge,
            Text.Truncate(item.JobCode, 64),
            Text.Truncate(item.Checksum, 120),
            Text.Truncate(item.Source, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPayBandAuditView> ToStrictPayBandAuditViews(List<StrictPayBandAudit> items)
    {
        var views = new List<StrictPayBandAuditView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPayBandAuditView(item));
        }
        return views;
    }

    public static double SumStrictPayBandAuditView(StrictPayBandAuditView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
