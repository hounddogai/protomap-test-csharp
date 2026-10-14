namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan variance.</summary>
public sealed record SuccessionPlanVarianceView(string Code, string Name, string Title, string Archived, string EmployeeIdText, string WindowDaysText, string ExpiresAtText, string Label, string Skill, string Weight, string Minimum, string StatusLabel)
{
    public static SuccessionPlanVarianceView ToSuccessionPlanVarianceView(SuccessionPlanVariance item)
    {
        return new SuccessionPlanVarianceView(
            item.Code,
            item.Name,
            item.Title,
            $"{item.Archived}",
            Text.Truncate(item.EmployeeId, 120),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Skill,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanVarianceView> ToSuccessionPlanVarianceViews(List<SuccessionPlanVariance> items)
    {
        var views = new List<SuccessionPlanVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanVarianceView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanVarianceView(SuccessionPlanVarianceView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedExpiresAt + parsedWeight;
    }
}
