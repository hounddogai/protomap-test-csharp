namespace Acme.Workforce.Compliance.Exception.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a pending exception slot.</summary>
public sealed record PendingExceptionSlotView(string Code, string Title, string BatchSizeText, string Version, string Visible, string Limit, string TeamText, string Amount, string StatusLabel)
{
    public static PendingExceptionSlotView ToPendingExceptionSlotView(PendingExceptionSlot item)
    {
        return new PendingExceptionSlotView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingExceptionSlotView> ToPendingExceptionSlotViews(List<PendingExceptionSlot> items)
    {
        var views = new List<PendingExceptionSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPendingExceptionSlotView(item));
        }
        return views;
    }

    public static double SumPendingExceptionSlotView(PendingExceptionSlotView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedVersion + (double)parsedLimit;
    }
}
