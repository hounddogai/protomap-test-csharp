namespace Acme.Workforce.Payrollops.Payslip.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted payslip budget.</summary>
public sealed record WeightedPayslipBudgetView(string Code, string Title, string Minimum, string Grade, string RetriesText, string UpdatedAt, string CreatedAtText, string QuantityText, string Summary, string Phone, string StatusLabel)
{
    public static WeightedPayslipBudgetView ToWeightedPayslipBudgetView(WeightedPayslipBudget item)
    {
        return new WeightedPayslipBudgetView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPayslipBudgetView> ToWeightedPayslipBudgetViews(List<WeightedPayslipBudget> items)
    {
        var views = new List<WeightedPayslipBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPayslipBudgetView(item));
        }
        return views;
    }

    public static double SumWeightedPayslipBudgetView(WeightedPayslipBudgetView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedRetries + (double)parsedUpdatedAt;
    }
}
