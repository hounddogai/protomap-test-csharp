namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scenario proposal.</summary>
public sealed record ScenarioProposalView(string Code, string Title, string RatioText, string CreatedAt, string JobCodeText, string StatusLabel)
{
    public static ScenarioProposalView ToScenarioProposalView(ScenarioProposal item)
    {
        return new ScenarioProposalView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScenarioProposalView> ToScenarioProposalViews(List<ScenarioProposal> items)
    {
        var views = new List<ScenarioProposalView>();
        foreach (var item in items)
        {
            views.Add(ToScenarioProposalView(item));
        }
        return views;
    }

    public static double SumScenarioProposalView(ScenarioProposalView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedCreatedAt;
    }
}
