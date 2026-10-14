namespace Acme.Workforce.Talent.Pool.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pool exception.</summary>
public sealed record RollingPoolExceptionView(string CodeText, string Label, string ExternalRefText, string LeaveDaysText, string BudgetText, string GraceDaysText, string StatusLabel)
{
    public static RollingPoolExceptionView ToRollingPoolExceptionView(RollingPoolException item)
    {
        return new RollingPoolExceptionView(
            item.Code,
            item.Label,
            Text.Truncate(item.ExternalRef, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPoolExceptionView> ToRollingPoolExceptionViews(List<RollingPoolException> items)
    {
        var views = new List<RollingPoolExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPoolExceptionView(item));
        }
        return views;
    }

    public static double SumRollingPoolExceptionView(RollingPoolExceptionView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedBudget + (double)parsedGraceDays;
    }
}
