namespace Acme.Workforce.Contractors.Offboarding.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding request.</summary>
public sealed record OffboardingRequestView(string Code, string TitleText, string BudgetText, string GradeText, string Ceiling, string DurationMinutes, string Locale, string Skill, string StatusLabel)
{
    public static OffboardingRequestView ToOffboardingRequestView(OffboardingRequest item)
    {
        return new OffboardingRequestView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 80),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingRequestView> ToOffboardingRequestViews(List<OffboardingRequest> items)
    {
        var views = new List<OffboardingRequestView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingRequestView(item));
        }
        return views;
    }

    public static double SumOffboardingRequestView(OffboardingRequestView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedBudget + parsedCeiling + (double)parsedDurationMinutes;
    }
}
