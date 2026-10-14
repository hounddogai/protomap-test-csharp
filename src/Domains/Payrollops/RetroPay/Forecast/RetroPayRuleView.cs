namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using System.Globalization;

/// <summary>How screens and exports show a retro pay rule.</summary>
public sealed record RetroPayRuleView(string Code, string Title, string Channel, string BudgetText, string EmployeeId, string StatusLabel)
{
    public static RetroPayRuleView ToRetroPayRuleView(RetroPayRule item)
    {
        return new RetroPayRuleView(
            item.Code,
            item.Title,
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayRuleView> ToRetroPayRuleViews(List<RetroPayRule> items)
    {
        var views = new List<RetroPayRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayRuleView(item));
        }
        return views;
    }

    public static double SumRetroPayRuleView(RetroPayRuleView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
