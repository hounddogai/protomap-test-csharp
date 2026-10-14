namespace Acme.Workforce.Compensation.BonusPool.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool cycle.</summary>
public sealed record BonusPoolCycleView(string Code, string LabelText, string Unit, string GraceDaysText, string ManagerId, string DepartmentText, string FloorValueText, string StatusLabel)
{
    public static BonusPoolCycleView ToBonusPoolCycleView(BonusPoolCycle item)
    {
        return new BonusPoolCycleView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Unit, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Department,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolCycleView> ToBonusPoolCycleViews(List<BonusPoolCycle> items)
    {
        var views = new List<BonusPoolCycleView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolCycleView(item));
        }
        return views;
    }

    public static double SumBonusPoolCycleView(BonusPoolCycleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFloorValue;
    }
}
