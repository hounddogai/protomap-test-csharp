namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary merit cycle slot.</summary>
public sealed record PrimaryMeritCycleSlotView(string Code, string Name, string Checksum, string Sequence, string ArchivedText, string StatusLabel)
{
    public static PrimaryMeritCycleSlotView ToPrimaryMeritCycleSlotView(PrimaryMeritCycleSlot item)
    {
        return new PrimaryMeritCycleSlotView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 80),
            item.Checksum,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryMeritCycleSlotView> ToPrimaryMeritCycleSlotViews(List<PrimaryMeritCycleSlot> items)
    {
        var views = new List<PrimaryMeritCycleSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryMeritCycleSlotView(item));
        }
        return views;
    }

    public static double SumPrimaryMeritCycleSlotView(PrimaryMeritCycleSlotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
