namespace Acme.Workforce.Compensation.BonusPool.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default bonus pool cycle.</summary>
public sealed record DefaultBonusPoolCycleView(string Code, string Label, string Discount, string Skill, string EmployeeId, string StatusLabel)
{
    public static DefaultBonusPoolCycleView ToDefaultBonusPoolCycleView(DefaultBonusPoolCycle item)
    {
        return new DefaultBonusPoolCycleView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBonusPoolCycleView> ToDefaultBonusPoolCycleViews(List<DefaultBonusPoolCycle> items)
    {
        var views = new List<DefaultBonusPoolCycleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBonusPoolCycleView(item));
        }
        return views;
    }

    public static double SumDefaultBonusPoolCycleView(DefaultBonusPoolCycleView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
