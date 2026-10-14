namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work rule.</summary>
public sealed record StatementOfWorkRuleView(string CodeText, string NameText, string Region, string Phone, string WindowDays, string EndsAtText, string TenureMonthsText, string AmountText, string StatusLabel)
{
    public static StatementOfWorkRuleView ToStatementOfWorkRuleView(StatementOfWorkRule item)
    {
        return new StatementOfWorkRuleView(
            item.Code,
            item.Name,
            item.Region,
            Text.Truncate(item.Phone, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkRuleView> ToStatementOfWorkRuleViews(List<StatementOfWorkRule> items)
    {
        var views = new List<StatementOfWorkRuleView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkRuleView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkRuleView(StatementOfWorkRuleView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedEndsAt + (double)parsedTenureMonths;
    }
}
