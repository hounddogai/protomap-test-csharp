namespace Acme.Workforce.Talent.SuccessionPlan.Request;

using System.Globalization;

/// <summary>How screens and exports show a global succession plan request.</summary>
public sealed record GlobalSuccessionPlanRequestView(string Code, string Label, string Priority, string ManagerId, string CostText, string Unit, string StatusLabel)
{
    public static GlobalSuccessionPlanRequestView ToGlobalSuccessionPlanRequestView(GlobalSuccessionPlanRequest item)
    {
        return new GlobalSuccessionPlanRequestView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalSuccessionPlanRequestView> ToGlobalSuccessionPlanRequestViews(List<GlobalSuccessionPlanRequest> items)
    {
        var views = new List<GlobalSuccessionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalSuccessionPlanRequestView(item));
        }
        return views;
    }

    public static double SumGlobalSuccessionPlanRequestView(GlobalSuccessionPlanRequestView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCost;
    }
}
