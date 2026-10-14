namespace Acme.Workforce.Orgdesign.Position.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy position plan.</summary>
public sealed record LegacyPositionPlanView(string Code, string NameText, string Weight, string CountryText, string TenureMonths, string Category, string Capacity, string ExternalRef, string Unit, string StatusLabel)
{
    public static LegacyPositionPlanView ToLegacyPositionPlanView(LegacyPositionPlan item)
    {
        return new LegacyPositionPlanView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPositionPlanView> ToLegacyPositionPlanViews(List<LegacyPositionPlan> items)
    {
        var views = new List<LegacyPositionPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPositionPlanView(item));
        }
        return views;
    }

    public static double SumLegacyPositionPlanView(LegacyPositionPlanView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedTenureMonths + (double)parsedCapacity;
    }
}
