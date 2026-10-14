namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip target.</summary>
public sealed record PayslipTargetView(string Code, string Label, string Cost, string Visible, string Score, string LimitText, string EndsAt, string RateText, string Phone, string Source, string Grade, string StatusLabel)
{
    public static PayslipTargetView ToPayslipTargetView(PayslipTarget item)
    {
        return new PayslipTargetView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 120),
            item.Source,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipTargetView> ToPayslipTargetViews(List<PayslipTarget> items)
    {
        var views = new List<PayslipTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipTargetView(item));
        }
        return views;
    }

    public static double SumPayslipTargetView(PayslipTargetView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return parsedCost + parsedScore + (double)parsedLimit;
    }
}
