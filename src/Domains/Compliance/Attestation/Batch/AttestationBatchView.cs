namespace Acme.Workforce.Compliance.Attestation.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation batch.</summary>
public sealed record AttestationBatchView(string Code, string Name, string ExternalRefText, string Hours, string Locale, string SegmentText, string SourceText, string TenureMonths, string Region, string EmployeeIdText, string StatusLabel)
{
    public static AttestationBatchView ToAttestationBatchView(AttestationBatch item)
    {
        return new AttestationBatchView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.ExternalRef,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Segment, 80),
            Text.Truncate(item.Source, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationBatchView> ToAttestationBatchViews(List<AttestationBatch> items)
    {
        var views = new List<AttestationBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationBatchView(item));
        }
        return views;
    }

    public static double SumAttestationBatchView(AttestationBatchView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedTenureMonths;
    }
}
