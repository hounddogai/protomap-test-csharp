namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched mobility allocation.</summary>
public sealed record BatchedMobilityAllocationView(string Code, string Title, string ExternalRefText, string SequenceText, string UpdatedAt, string Location, string SalaryBand, string Description, string Segment, string Priority, string StatusLabel)
{
    public static BatchedMobilityAllocationView ToBatchedMobilityAllocationView(BatchedMobilityAllocation item)
    {
        return new BatchedMobilityAllocationView(
            item.Code,
            item.Title,
            Text.Truncate(item.ExternalRef, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.SalaryBand,
            item.Description,
            item.Segment,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedMobilityAllocationView> ToBatchedMobilityAllocationViews(List<BatchedMobilityAllocation> items)
    {
        var views = new List<BatchedMobilityAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedMobilityAllocationView(item));
        }
        return views;
    }

    public static double SumBatchedMobilityAllocationView(BatchedMobilityAllocationView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedUpdatedAt + (double)parsedPriority;
    }
}
