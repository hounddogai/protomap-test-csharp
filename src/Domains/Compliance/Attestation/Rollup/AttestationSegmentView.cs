namespace Acme.Workforce.Compliance.Attestation.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a attestation segment.</summary>
public sealed record AttestationSegmentView(string CodeText, string Name, string Approved, string Label, string CeilingText, string SummaryText, string Location, string CostCenter, string Weight, string SalaryBand, string StatusLabel)
{
    public static AttestationSegmentView ToAttestationSegmentView(AttestationSegment item)
    {
        return new AttestationSegmentView(
            item.Code,
            item.Name,
            $"{item.Approved}",
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Location,
            item.CostCenter,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationSegmentView> ToAttestationSegmentViews(List<AttestationSegment> items)
    {
        var views = new List<AttestationSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationSegmentView(item));
        }
        return views;
    }

    public static double SumAttestationSegmentView(AttestationSegmentView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedWeight;
    }
}
