namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered merit cycle plan.</summary>
public sealed record TieredMeritCyclePlanView(string CodeText, string Name, string ManagerId, string Weight, string Department, string Budget, string Limit, string StatusLabel)
{
    public static TieredMeritCyclePlanView ToTieredMeritCyclePlanView(TieredMeritCyclePlan item)
    {
        return new TieredMeritCyclePlanView(
            item.Code,
            item.Name,
            Text.Truncate(item.ManagerId, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredMeritCyclePlanView> ToTieredMeritCyclePlanViews(List<TieredMeritCyclePlan> items)
    {
        var views = new List<TieredMeritCyclePlanView>();
        foreach (var item in items)
        {
            views.Add(ToTieredMeritCyclePlanView(item));
        }
        return views;
    }

    public static double SumTieredMeritCyclePlanView(TieredMeritCyclePlanView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedWeight + parsedBudget + (double)parsedLimit;
    }
}
