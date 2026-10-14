namespace Acme.Workforce.Assets.Assignment.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a assignment slot.</summary>
public sealed record AssignmentSlotView(string CodeText, string LabelText, string BatchSize, string CreatedAtText, string Priority, string OwnerId, string StatusLabel)
{
    public static AssignmentSlotView ToAssignmentSlotView(AssignmentSlot item)
    {
        return new AssignmentSlotView(
            item.Code,
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentSlotView> ToAssignmentSlotViews(List<AssignmentSlot> items)
    {
        var views = new List<AssignmentSlotView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentSlotView(item));
        }
        return views;
    }

    public static double SumAssignmentSlotView(AssignmentSlotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedCreatedAt + (double)parsedPriority;
    }
}
