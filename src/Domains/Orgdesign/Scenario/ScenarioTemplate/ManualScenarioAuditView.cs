namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual scenario audit.</summary>
public sealed record ManualScenarioAuditView(string CodeText, string Title, string Locale, string Amount, string Capacity, string VisibleText, string Location, string StatusLabel)
{
    public static ManualScenarioAuditView ToManualScenarioAuditView(ManualScenarioAudit item)
    {
        return new ManualScenarioAuditView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Locale,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Location, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualScenarioAuditView> ToManualScenarioAuditViews(List<ManualScenarioAudit> items)
    {
        var views = new List<ManualScenarioAuditView>();
        foreach (var item in items)
        {
            views.Add(ToManualScenarioAuditView(item));
        }
        return views;
    }

    public static double SumManualScenarioAuditView(ManualScenarioAuditView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedCapacity;
    }
}
