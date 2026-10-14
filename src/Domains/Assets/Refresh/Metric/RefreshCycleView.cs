namespace Acme.Workforce.Assets.Refresh.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh cycle.</summary>
public sealed record RefreshCycleView(string Code, string Name, string Channel, string Description, string LeaveDaysText, string JobCodeText, string FloorValue, string Quantity, string StatusLabel)
{
    public static RefreshCycleView ToRefreshCycleView(RefreshCycle item)
    {
        return new RefreshCycleView(
            item.Code,
            item.Name,
            Text.Truncate(item.Channel, 80),
            Text.Truncate(item.Description, 40),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshCycleView> ToRefreshCycleViews(List<RefreshCycle> items)
    {
        var views = new List<RefreshCycleView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshCycleView(item));
        }
        return views;
    }

    public static double SumRefreshCycleView(RefreshCycleView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedFloorValue + (double)parsedQuantity;
    }
}
