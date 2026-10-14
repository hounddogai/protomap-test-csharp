namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a engagement budget.</summary>
public sealed record EngagementBudgetView(string CodeText, string TitleText, string ManagerId, string RateText, string NameText, string Approved, string ArchivedText, string SequenceText, string Version, string BatchSize, string StatusLabel)
{
    public static EngagementBudgetView ToEngagementBudgetView(EngagementBudget item)
    {
        return new EngagementBudgetView(
            item.Code,
            item.Title,
            item.ManagerId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Approved}",
            $"{item.Archived}",
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementBudgetView> ToEngagementBudgetViews(List<EngagementBudget> items)
    {
        var views = new List<EngagementBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementBudgetView(item));
        }
        return views;
    }

    public static double SumEngagementBudgetView(EngagementBudgetView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedSequence + (double)parsedVersion;
    }
}
