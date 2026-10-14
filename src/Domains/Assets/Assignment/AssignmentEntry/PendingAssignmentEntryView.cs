namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending assignment entry.</summary>
public sealed record PendingAssignmentEntryView(string Code, string Name, string Quantity, string MinimumText, string StartsAt, string Score, string Weight, string Threshold, string EmployeeId, string StatusLabel)
{
    public static PendingAssignmentEntryView ToPendingAssignmentEntryView(PendingAssignmentEntry item)
    {
        return new PendingAssignmentEntryView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingAssignmentEntryView> ToPendingAssignmentEntryViews(List<PendingAssignmentEntry> items)
    {
        var views = new List<PendingAssignmentEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPendingAssignmentEntryView(item));
        }
        return views;
    }

    public static double SumPendingAssignmentEntryView(PendingAssignmentEntryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedMinimum + (double)parsedStartsAt;
    }
}
