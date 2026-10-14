namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a merit cycle slot.</summary>
public sealed record MeritCycleSlotView(string CodeText, string Title, string Threshold, string Revision, string CostCenter, string Unit, string Name, string NotesText, string Weight, string StatusLabel)
{
    public static MeritCycleSlotView ToMeritCycleSlotView(MeritCycleSlot item)
    {
        return new MeritCycleSlotView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Unit,
            item.Name,
            item.Notes,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleSlotView> ToMeritCycleSlotViews(List<MeritCycleSlot> items)
    {
        var views = new List<MeritCycleSlotView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleSlotView(item));
        }
        return views;
    }

    public static double SumMeritCycleSlotView(MeritCycleSlotView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRevision + parsedWeight;
    }
}
