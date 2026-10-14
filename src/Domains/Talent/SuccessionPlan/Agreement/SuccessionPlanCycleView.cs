namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan cycle.</summary>
public sealed record SuccessionPlanCycleView(string Code, string Label, string LocaleText, string ExpiresAt, string TenureMonthsText, string Segment, string Cost, string Revision, string Weight, string FloorValueText, string Maximum, string StatusLabel)
{
    public static SuccessionPlanCycleView ToSuccessionPlanCycleView(SuccessionPlanCycle item)
    {
        return new SuccessionPlanCycleView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Locale,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanCycleView> ToSuccessionPlanCycleViews(List<SuccessionPlanCycle> items)
    {
        var views = new List<SuccessionPlanCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanCycleView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanCycleView(SuccessionPlanCycleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedTenureMonths + parsedCost;
    }
}
