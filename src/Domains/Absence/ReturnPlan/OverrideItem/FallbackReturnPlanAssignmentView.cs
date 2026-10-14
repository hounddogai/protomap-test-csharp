namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback return plan assignment.</summary>
public sealed record FallbackReturnPlanAssignmentView(string Code, string LabelText, string Quantity, string GraceDays, string JobCode, string StatusLabel)
{
    public static FallbackReturnPlanAssignmentView ToFallbackReturnPlanAssignmentView(FallbackReturnPlanAssignment item)
    {
        return new FallbackReturnPlanAssignmentView(
            item.Code,
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackReturnPlanAssignmentView> ToFallbackReturnPlanAssignmentViews(List<FallbackReturnPlanAssignment> items)
    {
        var views = new List<FallbackReturnPlanAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackReturnPlanAssignmentView(item));
        }
        return views;
    }

    public static double SumFallbackReturnPlanAssignmentView(FallbackReturnPlanAssignmentView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedGraceDays;
    }
}
