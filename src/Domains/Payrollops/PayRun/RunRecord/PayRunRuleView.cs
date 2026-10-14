namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run rule.</summary>
public sealed record PayRunRuleView(string Code, string Title, string Country, string Sequence, string Cost, string EmployeeId, string EndsAt, string Team, string CategoryText, string StatusLabel)
{
    public static PayRunRuleView ToPayRunRuleView(PayRunRule item)
    {
        return new PayRunRuleView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Country,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunRuleView> ToPayRunRuleViews(List<PayRunRule> items)
    {
        var views = new List<PayRunRuleView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunRuleView(item));
        }
        return views;
    }

    public static double SumPayRunRuleView(PayRunRuleView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedCost + (double)parsedEndsAt;
    }
}
