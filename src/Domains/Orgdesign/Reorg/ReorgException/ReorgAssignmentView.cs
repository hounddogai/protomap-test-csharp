namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using System.Globalization;

/// <summary>How screens and exports show a reorg assignment.</summary>
public sealed record ReorgAssignmentView(string Code, string Title, string GraceDays, string BatchSizeText, string Margin, string Reviewer, string Retries, string Cost, string StatusLabel)
{
    public static ReorgAssignmentView ToReorgAssignmentView(ReorgAssignment item)
    {
        return new ReorgAssignmentView(
            item.Code,
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReorgAssignmentView> ToReorgAssignmentViews(List<ReorgAssignment> items)
    {
        var views = new List<ReorgAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToReorgAssignmentView(item));
        }
        return views;
    }

    public static double SumReorgAssignmentView(ReorgAssignmentView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedBatchSize + parsedMargin;
    }
}
