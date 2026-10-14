namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction rule.</summary>
public sealed record DeductionRuleView(string CodeText, string Name, string Limit, string Checksum, string DurationMinutesText, string ManagerId, string Team, string Version, string Unit, string StatusLabel)
{
    public static DeductionRuleView ToDeductionRuleView(DeductionRule item)
    {
        return new DeductionRuleView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Team,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionRuleView> ToDeductionRuleViews(List<DeductionRule> items)
    {
        var views = new List<DeductionRuleView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionRuleView(item));
        }
        return views;
    }

    public static double SumDeductionRuleView(DeductionRuleView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedDurationMinutes + (double)parsedVersion;
    }
}
