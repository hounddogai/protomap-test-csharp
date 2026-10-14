namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient action plan bundle.</summary>
public sealed record LenientActionPlanBundleView(string Code, string Name, string GraceDays, string EmployeeId, string Capacity, string Location, string TenureMonths, string Visible, string SkillText, string BatchSize, string StatusLabel)
{
    public static LenientActionPlanBundleView ToLenientActionPlanBundleView(LenientActionPlanBundle item)
    {
        return new LenientActionPlanBundleView(
            item.Code,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Skill,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientActionPlanBundleView> ToLenientActionPlanBundleViews(List<LenientActionPlanBundle> items)
    {
        var views = new List<LenientActionPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientActionPlanBundleView(item));
        }
        return views;
    }

    public static double SumLenientActionPlanBundleView(LenientActionPlanBundleView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedCapacity + (double)parsedTenureMonths;
    }
}
