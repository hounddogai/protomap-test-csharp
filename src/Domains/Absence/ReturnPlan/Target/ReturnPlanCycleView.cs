namespace Acme.Workforce.Absence.ReturnPlan.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a return plan cycle.</summary>
public sealed record ReturnPlanCycleView(string Code, string TitleText, string Grade, string Priority, string Unit, string Amount, string OwnerId, string Active, string StatusLabel)
{
    public static ReturnPlanCycleView ToReturnPlanCycleView(ReturnPlanCycle item)
    {
        return new ReturnPlanCycleView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.Grade, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanCycleView> ToReturnPlanCycleViews(List<ReturnPlanCycle> items)
    {
        var views = new List<ReturnPlanCycleView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanCycleView(item));
        }
        return views;
    }

    public static double SumReturnPlanCycleView(ReturnPlanCycleView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedAmount;
    }
}
