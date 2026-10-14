namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a reorg baseline.</summary>
public sealed record ReorgBaselineView(string Code, string Title, string Capacity, string PhoneText, string Country, string SkillText, string Timezone, string EmployeeId, string Retries, string StatusLabel)
{
    public static ReorgBaselineView ToReorgBaselineView(ReorgBaseline item)
    {
        return new ReorgBaselineView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Country,
            Text.Truncate(item.Skill, 120),
            item.Timezone,
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgBaselineView> ToReorgBaselineViews(List<ReorgBaseline> items)
    {
        var views = new List<ReorgBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToReorgBaselineView(item));
        }
        return views;
    }

    public static double SumReorgBaselineView(ReorgBaselineView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedRetries;
    }
}
