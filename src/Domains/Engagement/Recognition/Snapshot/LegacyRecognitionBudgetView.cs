namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a legacy recognition budget.</summary>
public sealed record LegacyRecognitionBudgetView(string Code, string NameText, string Budget, string Retries, string TeamText, string UpdatedAtText, string ReviewerText, string StatusLabel)
{
    public static LegacyRecognitionBudgetView ToLegacyRecognitionBudgetView(LegacyRecognitionBudget item)
    {
        return new LegacyRecognitionBudgetView(
            item.Code,
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRecognitionBudgetView> ToLegacyRecognitionBudgetViews(List<LegacyRecognitionBudget> items)
    {
        var views = new List<LegacyRecognitionBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRecognitionBudgetView(item));
        }
        return views;
    }

    public static double SumLegacyRecognitionBudgetView(LegacyRecognitionBudgetView view)
    {
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedRetries + (double)parsedUpdatedAt;
    }
}
