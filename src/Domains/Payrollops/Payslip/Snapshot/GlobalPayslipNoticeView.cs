namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a global payslip notice.</summary>
public sealed record GlobalPayslipNoticeView(string Code, string NameText, string Country, string Unit, string Maximum, string Budget, string BadgeText, string Amount, string TitleText, string QuantityText, string StatusLabel)
{
    public static GlobalPayslipNoticeView ToGlobalPayslipNoticeView(GlobalPayslipNotice item)
    {
        return new GlobalPayslipNoticeView(
            item.Code,
            item.Name,
            item.Country,
            item.Unit,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPayslipNoticeView> ToGlobalPayslipNoticeViews(List<GlobalPayslipNotice> items)
    {
        var views = new List<GlobalPayslipNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPayslipNoticeView(item));
        }
        return views;
    }

    public static double SumGlobalPayslipNoticeView(GlobalPayslipNoticeView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedBudget + parsedAmount;
    }
}
