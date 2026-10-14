namespace Acme.Workforce.Absence.Accrual.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual summary.</summary>
public sealed record AccrualSummaryView(string Code, string Name, string VisibleText, string ContactEmailText, string Skill, string Reviewer, string CurrencyText, string SummaryText, string Quantity, string StatusLabel)
{
    public static AccrualSummaryView ToAccrualSummaryView(AccrualSummary item)
    {
        return new AccrualSummaryView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 40),
            $"{item.Visible}",
            Text.Truncate(item.ContactEmail, 40),
            item.Skill,
            Text.Truncate(item.Reviewer, 120),
            item.Currency,
            item.Summary,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualSummaryView> ToAccrualSummaryViews(List<AccrualSummary> items)
    {
        var views = new List<AccrualSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualSummaryView(item));
        }
        return views;
    }

    public static double SumAccrualSummaryView(AccrualSummaryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
