namespace Acme.Workforce.Workplace.Visitor.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor allocation.</summary>
public sealed record VisitorAllocationView(string Code, string LabelText, string ChannelText, string GraceDays, string EmployeeIdText, string FloorValue, string StatusLabel)
{
    public static VisitorAllocationView ToVisitorAllocationView(VisitorAllocation item)
    {
        return new VisitorAllocationView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Channel,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorAllocationView> ToVisitorAllocationViews(List<VisitorAllocation> items)
    {
        var views = new List<VisitorAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorAllocationView(item));
        }
        return views;
    }

    public static double SumVisitorAllocationView(VisitorAllocationView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedFloorValue;
    }
}
