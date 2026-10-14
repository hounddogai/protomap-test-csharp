namespace Acme.Workforce.Compliance.Audit.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit profile.</summary>
public sealed record AuditProfileView(string Code, string LabelText, string TenureMonths, string Discount, string Score, string LeaveDays, string WindowDays, string SalaryBand, string Locale, string SequenceText, string StatusLabel)
{
    public static AuditProfileView ToAuditProfileView(AuditProfile item)
    {
        return new AuditProfileView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 40),
            item.Locale,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditProfileView> ToAuditProfileViews(List<AuditProfile> items)
    {
        var views = new List<AuditProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAuditProfileView(item));
        }
        return views;
    }

    public static double SumAuditProfileView(AuditProfileView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedDiscount + parsedScore;
    }
}
