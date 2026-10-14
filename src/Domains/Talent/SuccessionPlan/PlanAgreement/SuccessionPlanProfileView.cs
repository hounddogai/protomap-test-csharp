namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan profile.</summary>
public sealed record SuccessionPlanProfileView(string CodeText, string Title, string Capacity, string Segment, string EndsAtText, string StatusLabel)
{
    public static SuccessionPlanProfileView ToSuccessionPlanProfileView(SuccessionPlanProfile item)
    {
        return new SuccessionPlanProfileView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanProfileView> ToSuccessionPlanProfileViews(List<SuccessionPlanProfile> items)
    {
        var views = new List<SuccessionPlanProfileView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanProfileView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanProfileView(SuccessionPlanProfileView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedEndsAt;
    }
}
