namespace Acme.Workforce.Assets.Assignment.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a assignment override.</summary>
public sealed record AssignmentOverrideView(string Code, string Label, string Pinned, string QuantityText, string TeamText, string ApprovedText, string Source, string Ceiling, string ExternalRef, string StatusLabel)
{
    public static AssignmentOverrideView ToAssignmentOverrideView(AssignmentOverride item)
    {
        return new AssignmentOverrideView(
            item.Code,
            item.Label,
            $"{item.Pinned}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Approved}",
            item.Source,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentOverrideView> ToAssignmentOverrideViews(List<AssignmentOverride> items)
    {
        var views = new List<AssignmentOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentOverrideView(item));
        }
        return views;
    }

    public static double SumAssignmentOverrideView(AssignmentOverrideView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedCeiling;
    }
}
