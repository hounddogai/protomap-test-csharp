namespace Acme.Workforce.Absence.Holiday.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending holiday snapshot.</summary>
public sealed record PendingHolidaySnapshotView(string Code, string Name, string Fte, string Budget, string Priority, string ContactEmailText, string FloorValue, string Currency, string Rating, string Amount, string StatusLabel)
{
    public static PendingHolidaySnapshotView ToPendingHolidaySnapshotView(PendingHolidaySnapshot item)
    {
        return new PendingHolidaySnapshotView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingHolidaySnapshotView> ToPendingHolidaySnapshotViews(List<PendingHolidaySnapshot> items)
    {
        var views = new List<PendingHolidaySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPendingHolidaySnapshotView(item));
        }
        return views;
    }

    public static double SumPendingHolidaySnapshotView(PendingHolidaySnapshotView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedFte + parsedBudget + (double)parsedPriority;
    }
}
