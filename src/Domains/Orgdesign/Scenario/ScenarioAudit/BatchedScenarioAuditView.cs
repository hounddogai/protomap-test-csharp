namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using System.Globalization;

/// <summary>How screens and exports show a batched scenario audit.</summary>
public sealed record BatchedScenarioAuditView(string Code, string LabelText, string Pinned, string FteText, string Version, string BatchSize, string Active, string GraceDays, string RetriesText, string Skill, string StatusLabel)
{
    public static BatchedScenarioAuditView ToBatchedScenarioAuditView(BatchedScenarioAudit item)
    {
        return new BatchedScenarioAuditView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedScenarioAuditView> ToBatchedScenarioAuditViews(List<BatchedScenarioAudit> items)
    {
        var views = new List<BatchedScenarioAuditView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedScenarioAuditView(item));
        }
        return views;
    }

    public static double SumBatchedScenarioAuditView(BatchedScenarioAuditView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedVersion + (double)parsedBatchSize;
    }
}
