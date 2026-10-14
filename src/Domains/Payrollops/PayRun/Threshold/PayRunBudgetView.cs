namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a pay run budget.</summary>
public sealed record PayRunBudgetView(string Code, string Name, string GradeText, string Amount, string MarginText, string LeaveDays, string ScoreText, string FteText, string StatusLabel)
{
    public static PayRunBudgetView ToPayRunBudgetView(PayRunBudget item)
    {
        return new PayRunBudgetView(
            item.Code,
            item.Name,
            item.Grade,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunBudgetView> ToPayRunBudgetViews(List<PayRunBudget> items)
    {
        var views = new List<PayRunBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunBudgetView(item));
        }
        return views;
    }

    public static double SumPayRunBudgetView(PayRunBudgetView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMargin + (double)parsedLeaveDays;
    }
}
