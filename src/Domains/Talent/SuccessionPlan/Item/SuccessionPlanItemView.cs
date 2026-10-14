namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using System.Globalization;

/// <summary>How screens and exports show a succession plan item.</summary>
public sealed record SuccessionPlanItemView(string Code, string Title, string Source, string BatchSize, string Priority, string StatusLabel)
{
    public static SuccessionPlanItemView ToSuccessionPlanItemView(SuccessionPlanItem item)
    {
        return new SuccessionPlanItemView(
            item.Code,
            item.Title,
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanItemView> ToSuccessionPlanItemViews(List<SuccessionPlanItem> items)
    {
        var views = new List<SuccessionPlanItemView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanItemView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanItemView(SuccessionPlanItemView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedPriority;
    }
}
