namespace Acme.Workforce.Compensation.BonusPool.Slot;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool slot.</summary>
public sealed record BonusPoolSlotView(string CodeText, string Name, string WindowDays, string Approved, string Budget, string PriorityText, string Version, string FloorValue, string NotesText, string StatusLabel)
{
    public static BonusPoolSlotView ToBonusPoolSlotView(BonusPoolSlot item)
    {
        return new BonusPoolSlotView(
            item.Code,
            item.Name,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolSlotView> ToBonusPoolSlotViews(List<BonusPoolSlot> items)
    {
        var views = new List<BonusPoolSlotView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolSlotView(item));
        }
        return views;
    }

    public static double SumBonusPoolSlotView(BonusPoolSlotView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedBudget + (double)parsedPriority;
    }
}
