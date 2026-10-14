namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient garnishment budget.</summary>
public sealed record LenientGarnishmentBudgetView(string Code, string Label, string Checksum, string Priority, string AmountText, string MaximumText, string BadgeText, string StatusLabel)
{
    public static LenientGarnishmentBudgetView ToLenientGarnishmentBudgetView(LenientGarnishmentBudget item)
    {
        return new LenientGarnishmentBudgetView(
            item.Code,
            item.Label,
            Text.Truncate(item.Checksum, 40),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientGarnishmentBudgetView> ToLenientGarnishmentBudgetViews(List<LenientGarnishmentBudget> items)
    {
        var views = new List<LenientGarnishmentBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLenientGarnishmentBudgetView(item));
        }
        return views;
    }

    public static double SumLenientGarnishmentBudgetView(LenientGarnishmentBudgetView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedAmount + parsedMaximum;
    }
}
