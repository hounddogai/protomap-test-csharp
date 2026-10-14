namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient accrual slot.</summary>
public sealed record LenientAccrualSlotView(string Code, string Label, string BatchSizeText, string ContactEmail, string WindowDays, string ThresholdText, string VersionText, string StatusLabel)
{
    public static LenientAccrualSlotView ToLenientAccrualSlotView(LenientAccrualSlot item)
    {
        return new LenientAccrualSlotView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAccrualSlotView> ToLenientAccrualSlotViews(List<LenientAccrualSlot> items)
    {
        var views = new List<LenientAccrualSlotView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAccrualSlotView(item));
        }
        return views;
    }

    public static double SumLenientAccrualSlotView(LenientAccrualSlotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedWindowDays + parsedThreshold;
    }
}
