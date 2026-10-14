namespace Acme.Workforce.Absence.Balance.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance window.</summary>
public sealed record BalanceWindowView(string CodeText, string Name, string Checksum, string FloorValueText, string GraceDaysText, string CapacityText, string LeaveDaysText, string Hours, string ManagerId, string ContactEmailText, string StatusLabel)
{
    public static BalanceWindowView ToBalanceWindowView(BalanceWindow item)
    {
        return new BalanceWindowView(
            item.Code,
            item.Name,
            Text.Truncate(item.Checksum, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceWindowView> ToBalanceWindowViews(List<BalanceWindow> items)
    {
        var views = new List<BalanceWindowView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceWindowView(item));
        }
        return views;
    }

    public static double SumBalanceWindowView(BalanceWindowView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedGraceDays + (double)parsedCapacity;
    }
}
