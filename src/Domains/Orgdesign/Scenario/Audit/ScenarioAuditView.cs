namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario audit.</summary>
public sealed record ScenarioAuditView(string CodeText, string Title, string Revision, string BatchSizeText, string FloorValue, string Category, string Phone, string Sequence, string AttemptsText, string ArchivedText, string StatusLabel)
{
    public static ScenarioAuditView ToScenarioAuditView(ScenarioAudit item)
    {
        return new ScenarioAuditView(
            item.Code,
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            Text.Truncate(item.Phone, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioAuditView> ToScenarioAuditViews(List<ScenarioAudit> items)
    {
        var views = new List<ScenarioAuditView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioAuditView(item));
        }
        return views;
    }

    public static double SumScenarioAuditView(ScenarioAuditView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedBatchSize + parsedFloorValue;
    }
}
