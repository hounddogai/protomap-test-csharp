namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario agreement.</summary>
public sealed record ScenarioAgreementView(string Code, string Name, string BatchSize, string PriorityText, string Quantity, string PinnedText, string LabelText, string StatusLabel)
{
    public static ScenarioAgreementView ToScenarioAgreementView(ScenarioAgreement item)
    {
        return new ScenarioAgreementView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioAgreementView> ToScenarioAgreementViews(List<ScenarioAgreement> items)
    {
        var views = new List<ScenarioAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioAgreementView(item));
        }
        return views;
    }

    public static double SumScenarioAgreementView(ScenarioAgreementView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedPriority + (double)parsedQuantity;
    }
}
