namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient deduction notice.</summary>
public sealed record LenientDeductionNoticeView(string Code, string Label, string Grade, string CostCenter, string SalaryBand, string TenureMonths, string StatusLabel)
{
    public static LenientDeductionNoticeView ToLenientDeductionNoticeView(LenientDeductionNotice item)
    {
        return new LenientDeductionNoticeView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Grade,
            item.CostCenter,
            Text.Truncate(item.SalaryBand, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientDeductionNoticeView> ToLenientDeductionNoticeViews(List<LenientDeductionNotice> items)
    {
        var views = new List<LenientDeductionNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToLenientDeductionNoticeView(item));
        }
        return views;
    }

    public static double SumLenientDeductionNoticeView(LenientDeductionNoticeView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
