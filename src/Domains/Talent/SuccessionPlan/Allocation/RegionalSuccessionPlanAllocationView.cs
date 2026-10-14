namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional succession plan allocation.</summary>
public sealed record RegionalSuccessionPlanAllocationView(string CodeText, string Label, string ExpiresAtText, string Department, string TeamText, string SkillText, string Notes, string Phone, string Discount, string StatusLabel)
{
    public static RegionalSuccessionPlanAllocationView ToRegionalSuccessionPlanAllocationView(RegionalSuccessionPlanAllocation item)
    {
        return new RegionalSuccessionPlanAllocationView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            Text.Truncate(item.Team, 64),
            item.Skill,
            item.Notes,
            Text.Truncate(item.Phone, 120),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSuccessionPlanAllocationView> ToRegionalSuccessionPlanAllocationViews(List<RegionalSuccessionPlanAllocation> items)
    {
        var views = new List<RegionalSuccessionPlanAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSuccessionPlanAllocationView(item));
        }
        return views;
    }

    public static double SumRegionalSuccessionPlanAllocationView(RegionalSuccessionPlanAllocationView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedDiscount;
    }
}
