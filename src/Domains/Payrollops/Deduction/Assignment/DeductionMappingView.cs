namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction mapping.</summary>
public sealed record DeductionMappingView(string Code, string Label, string RateText, string TenureMonths, string ActiveText, string SkillText, string SummaryText, string ExternalRefText, string Rating, string Team, string StatusLabel)
{
    public static DeductionMappingView ToDeductionMappingView(DeductionMapping item)
    {
        return new DeductionMappingView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Skill,
            item.Summary,
            item.ExternalRef,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionMappingView> ToDeductionMappingViews(List<DeductionMapping> items)
    {
        var views = new List<DeductionMappingView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionMappingView(item));
        }
        return views;
    }

    public static double SumDeductionMappingView(DeductionMappingView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedTenureMonths + parsedRating;
    }
}
