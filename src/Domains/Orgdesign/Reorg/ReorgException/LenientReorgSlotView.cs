namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient reorg slot.</summary>
public sealed record LenientReorgSlotView(string Code, string Label, string SalaryBandText, string ExternalRef, string Sequence, string Name, string Budget, string Notes, string Attempts, string StatusLabel)
{
    public static LenientReorgSlotView ToLenientReorgSlotView(LenientReorgSlot item)
    {
        return new LenientReorgSlotView(
            item.Code,
            item.Label,
            item.SalaryBand,
            Text.Truncate(item.ExternalRef, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientReorgSlotView> ToLenientReorgSlotViews(List<LenientReorgSlot> items)
    {
        var views = new List<LenientReorgSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLenientReorgSlotView(item));
        }
        return views;
    }

    public static double SumLenientReorgSlotView(LenientReorgSlotView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedBudget + (double)parsedAttempts;
    }
}
