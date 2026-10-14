namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled payslip notice.</summary>
public sealed record ScheduledPayslipNoticeView(string CodeText, string Name, string Label, string Ratio, string Revision, string WindowDays, string SourceText, string Rate, string StatusLabel)
{
    public static ScheduledPayslipNoticeView ToScheduledPayslipNoticeView(ScheduledPayslipNotice item)
    {
        return new ScheduledPayslipNoticeView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayslipNoticeView> ToScheduledPayslipNoticeViews(List<ScheduledPayslipNotice> items)
    {
        var views = new List<ScheduledPayslipNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayslipNoticeView(item));
        }
        return views;
    }

    public static double SumScheduledPayslipNoticeView(ScheduledPayslipNoticeView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedRevision + (double)parsedWindowDays;
    }
}
