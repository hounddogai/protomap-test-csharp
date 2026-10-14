namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary visitor assignment.</summary>
public sealed record PrimaryVisitorAssignmentView(string Code, string LabelText, string WindowDaysText, string NameText, string CapacityText, string StatusLabel)
{
    public static PrimaryVisitorAssignmentView ToPrimaryVisitorAssignmentView(PrimaryVisitorAssignment item)
    {
        return new PrimaryVisitorAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryVisitorAssignmentView> ToPrimaryVisitorAssignmentViews(List<PrimaryVisitorAssignment> items)
    {
        var views = new List<PrimaryVisitorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryVisitorAssignmentView(item));
        }
        return views;
    }

    public static double SumPrimaryVisitorAssignmentView(PrimaryVisitorAssignmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedCapacity;
    }
}
