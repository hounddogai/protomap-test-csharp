namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary attestation entry.</summary>
public sealed record PrimaryAttestationEntryView(string Code, string LabelText, string TenureMonths, string Fte, string Reviewer, string Category, string RetriesText, string Capacity, string CreatedAtText, string StatusLabel)
{
    public static PrimaryAttestationEntryView ToPrimaryAttestationEntryView(PrimaryAttestationEntry item)
    {
        return new PrimaryAttestationEntryView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            Text.Truncate(item.Category, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAttestationEntryView> ToPrimaryAttestationEntryViews(List<PrimaryAttestationEntry> items)
    {
        var views = new List<PrimaryAttestationEntryView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAttestationEntryView(item));
        }
        return views;
    }

    public static double SumPrimaryAttestationEntryView(PrimaryAttestationEntryView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedFte + (double)parsedRetries;
    }
}
