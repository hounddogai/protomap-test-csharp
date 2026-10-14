namespace Acme.Workforce.Workplace.Visitor.Cycle;

using System.Globalization;

/// <summary>How screens and exports show a default visitor rule.</summary>
public sealed record DefaultVisitorRuleView(string Code, string Label, string PriorityText, string Name, string EmployeeIdText, string Segment, string AttemptsText, string GraceDaysText, string Capacity, string StartsAtText, string Score, string StatusLabel)
{
    public static DefaultVisitorRuleView ToDefaultVisitorRuleView(DefaultVisitorRule item)
    {
        return new DefaultVisitorRuleView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.EmployeeId,
            item.Segment,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVisitorRuleView> ToDefaultVisitorRuleViews(List<DefaultVisitorRule> items)
    {
        var views = new List<DefaultVisitorRuleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVisitorRuleView(item));
        }
        return views;
    }

    public static double SumDefaultVisitorRuleView(DefaultVisitorRuleView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedAttempts + (double)parsedGraceDays;
    }
}
