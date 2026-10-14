namespace Acme.Workforce.Orgdesign.Span.Preference;

using System.Globalization;

/// <summary>How screens and exports show a lenient span ledger.</summary>
public sealed record LenientSpanLedgerView(string Code, string Label, string Visible, string UpdatedAt, string Revision, string Hours, string EndsAt, string StatusLabel)
{
    public static LenientSpanLedgerView ToLenientSpanLedgerView(LenientSpanLedger item)
    {
        return new LenientSpanLedgerView(
            item.Code,
            item.Label,
            $"{item.Visible}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSpanLedgerView> ToLenientSpanLedgerViews(List<LenientSpanLedger> items)
    {
        var views = new List<LenientSpanLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSpanLedgerView(item));
        }
        return views;
    }

    public static double SumLenientSpanLedgerView(LenientSpanLedgerView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRevision + parsedHours;
    }
}
