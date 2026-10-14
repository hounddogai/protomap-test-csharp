namespace Acme.Workforce.Payrollops.Payslip.Notice;

using System.Globalization;

/// <summary>How screens and exports show a payslip notice.</summary>
public sealed record PayslipNoticeView(string Code, string Label, string Priority, string DescriptionText, string Version, string ScoreText, string StatusLabel)
{
    public static PayslipNoticeView ToPayslipNoticeView(PayslipNotice item)
    {
        return new PayslipNoticeView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipNoticeView> ToPayslipNoticeViews(List<PayslipNotice> items)
    {
        var views = new List<PayslipNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipNoticeView(item));
        }
        return views;
    }

    public static double SumPayslipNoticeView(PayslipNoticeView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedVersion + parsedScore;
    }
}
