namespace Acme.Workforce.Compliance.Attestation.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation entry.</summary>
public sealed record AttestationEntryView(string Code, string Label, string Source, string Fte, string Currency, string Title, string TenureMonths, string UpdatedAt, string StatusLabel)
{
    public static AttestationEntryView ToAttestationEntryView(AttestationEntry item)
    {
        return new AttestationEntryView(
            Text.Truncate(item.Code, 120),
            item.Label,
            Text.Truncate(item.Source, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationEntryView> ToAttestationEntryViews(List<AttestationEntry> items)
    {
        var views = new List<AttestationEntryView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationEntryView(item));
        }
        return views;
    }

    public static double SumAttestationEntryView(AttestationEntryView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedTenureMonths + (double)parsedUpdatedAt;
    }
}
