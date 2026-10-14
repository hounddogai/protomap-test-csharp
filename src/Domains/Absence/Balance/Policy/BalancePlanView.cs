namespace Acme.Workforce.Absence.Balance.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance plan.</summary>
public sealed record BalancePlanView(string Code, string Title, string ContactEmail, string Ceiling, string DurationMinutes, string StatusLabel)
{
    public static BalancePlanView ToBalancePlanView(BalancePlan item)
    {
        return new BalancePlanView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.ContactEmail, 120),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalancePlanView> ToBalancePlanViews(List<BalancePlan> items)
    {
        var views = new List<BalancePlanView>();
        foreach (var item in items)
        {
            views.Add(ToBalancePlanView(item));
        }
        return views;
    }

    public static double SumBalancePlanView(BalancePlanView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedDurationMinutes;
    }
}
