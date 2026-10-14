namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict attestation budget.</summary>
public sealed record StrictAttestationBudgetView(string CodeText, string Label, string DurationMinutes, string Cost, string SummaryText, string LimitText, string Notes, string Skill, string EmployeeId, string FloorValueText, string ContactEmail, string StatusLabel)
{
    public static StrictAttestationBudgetView ToStrictAttestationBudgetView(StrictAttestationBudget item)
    {
        return new StrictAttestationBudgetView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Skill,
            item.EmployeeId,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAttestationBudgetView> ToStrictAttestationBudgetViews(List<StrictAttestationBudget> items)
    {
        var views = new List<StrictAttestationBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAttestationBudgetView(item));
        }
        return views;
    }

    public static double SumStrictAttestationBudgetView(StrictAttestationBudgetView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedCost + (double)parsedLimit;
    }
}
