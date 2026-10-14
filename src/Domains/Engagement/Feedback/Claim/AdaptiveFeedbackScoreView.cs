namespace Acme.Workforce.Engagement.Feedback.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive feedback score.</summary>
public sealed record AdaptiveFeedbackScoreView(string CodeText, string Title, string Archived, string Budget, string Badge, string CostCenter, string Country, string Department, string StatusLabel)
{
    public static AdaptiveFeedbackScoreView ToAdaptiveFeedbackScoreView(AdaptiveFeedbackScore item)
    {
        return new AdaptiveFeedbackScoreView(
            Text.Truncate(item.Code, 120),
            item.Title,
            $"{item.Archived}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.CostCenter, 64),
            Text.Truncate(item.Country, 120),
            Text.Truncate(item.Department, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveFeedbackScoreView> ToAdaptiveFeedbackScoreViews(List<AdaptiveFeedbackScore> items)
    {
        var views = new List<AdaptiveFeedbackScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveFeedbackScoreView(item));
        }
        return views;
    }

    public static double SumAdaptiveFeedbackScoreView(AdaptiveFeedbackScoreView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedBudget;
    }
}
