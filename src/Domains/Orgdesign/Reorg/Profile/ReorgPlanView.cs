namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg plan.</summary>
public sealed record ReorgPlanView(string Code, string Label, string StartsAt, string Limit, string EmployeeId, string NameText, string StatusLabel)
{
    public static ReorgPlanView ToReorgPlanView(ReorgPlan item)
    {
        return new ReorgPlanView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Name, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgPlanView> ToReorgPlanViews(List<ReorgPlan> items)
    {
        var views = new List<ReorgPlanView>();
        foreach (var item in items)
        {
            views.Add(ToReorgPlanView(item));
        }
        return views;
    }

    public static double SumReorgPlanView(ReorgPlanView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedLimit;
    }
}
