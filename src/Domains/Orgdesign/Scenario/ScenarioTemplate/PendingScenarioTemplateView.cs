namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending scenario template.</summary>
public sealed record PendingScenarioTemplateView(string CodeText, string Name, string Unit, string CeilingText, string SalaryBand, string Checksum, string DurationMinutes, string EndsAtText, string Currency, string FloorValueText, string OwnerId, string StatusLabel)
{
    public static PendingScenarioTemplateView ToPendingScenarioTemplateView(PendingScenarioTemplate item)
    {
        return new PendingScenarioTemplateView(
            item.Code,
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Unit, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            Text.Truncate(item.Checksum, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingScenarioTemplateView> ToPendingScenarioTemplateViews(List<PendingScenarioTemplate> items)
    {
        var views = new List<PendingScenarioTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPendingScenarioTemplateView(item));
        }
        return views;
    }

    public static double SumPendingScenarioTemplateView(PendingScenarioTemplateView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedDurationMinutes + (double)parsedEndsAt;
    }
}
