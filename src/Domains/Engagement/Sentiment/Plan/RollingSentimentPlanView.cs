namespace Acme.Workforce.Engagement.Sentiment.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling sentiment plan.</summary>
public sealed record RollingSentimentPlanView(string CodeText, string Name, string ThresholdText, string Budget, string MinimumText, string ReviewerText, string RetriesText, string Unit, string BatchSize, string Description, string StatusLabel)
{
    public static RollingSentimentPlanView ToRollingSentimentPlanView(RollingSentimentPlan item)
    {
        return new RollingSentimentPlanView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 40),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSentimentPlanView> ToRollingSentimentPlanViews(List<RollingSentimentPlan> items)
    {
        var views = new List<RollingSentimentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSentimentPlanView(item));
        }
        return views;
    }

    public static double SumRollingSentimentPlanView(RollingSentimentPlanView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedBudget + parsedMinimum;
    }
}
