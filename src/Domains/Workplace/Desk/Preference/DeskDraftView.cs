namespace Acme.Workforce.Workplace.Desk.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk draft.</summary>
public sealed record DeskDraftView(string Code, string Title, string Attempts, string ChecksumText, string Grade, string BatchSizeText, string FteText, string VersionText, string QuantityText, string StatusLabel)
{
    public static DeskDraftView ToDeskDraftView(DeskDraft item)
    {
        return new DeskDraftView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Grade, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskDraftView> ToDeskDraftViews(List<DeskDraft> items)
    {
        var views = new List<DeskDraftView>();
        foreach (var item in items)
        {
            views.Add(ToDeskDraftView(item));
        }
        return views;
    }

    public static double SumDeskDraftView(DeskDraftView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedBatchSize + parsedFte;
    }
}
