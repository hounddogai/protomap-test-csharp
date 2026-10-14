namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip exception.</summary>
public sealed record PayslipExceptionView(string Code, string Title, string ApprovedText, string ContactEmailText, string AttemptsText, string Cost, string ChannelText, string Amount, string Name, string StatusLabel)
{
    public static PayslipExceptionView ToPayslipExceptionView(PayslipException item)
    {
        return new PayslipExceptionView(
            item.Code,
            Text.Truncate(item.Title, 40),
            $"{item.Approved}",
            Text.Truncate(item.ContactEmail, 120),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipExceptionView> ToPayslipExceptionViews(List<PayslipException> items)
    {
        var views = new List<PayslipExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipExceptionView(item));
        }
        return views;
    }

    public static double SumPayslipExceptionView(PayslipExceptionView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCost + parsedAmount;
    }
}
