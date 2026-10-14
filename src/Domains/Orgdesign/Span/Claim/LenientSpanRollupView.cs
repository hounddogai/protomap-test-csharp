namespace Acme.Workforce.Orgdesign.Span.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient span rollup.</summary>
public sealed record LenientSpanRollupView(string CodeText, string Name, string EmployeeId, string LeaveDays, string CategoryText, string Country, string AttemptsText, string Budget, string StatusLabel)
{
    public static LenientSpanRollupView ToLenientSpanRollupView(LenientSpanRollup item)
    {
        return new LenientSpanRollupView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.EmployeeId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Country,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSpanRollupView> ToLenientSpanRollupViews(List<LenientSpanRollup> items)
    {
        var views = new List<LenientSpanRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSpanRollupView(item));
        }
        return views;
    }

    public static double SumLenientSpanRollupView(LenientSpanRollupView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedAttempts + parsedBudget;
    }
}
