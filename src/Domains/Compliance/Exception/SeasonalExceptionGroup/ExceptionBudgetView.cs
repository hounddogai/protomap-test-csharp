namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using System.Globalization;

/// <summary>How screens and exports show a exception budget.</summary>
public sealed record ExceptionBudgetView(string CodeText, string Name, string WindowDaysText, string StartsAt, string Segment, string StatusLabel)
{
    public static ExceptionBudgetView ToExceptionBudgetView(ExceptionBudget item)
    {
        return new ExceptionBudgetView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionBudgetView> ToExceptionBudgetViews(List<ExceptionBudget> items)
    {
        var views = new List<ExceptionBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionBudgetView(item));
        }
        return views;
    }

    public static double SumExceptionBudgetView(ExceptionBudgetView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedStartsAt;
    }
}
