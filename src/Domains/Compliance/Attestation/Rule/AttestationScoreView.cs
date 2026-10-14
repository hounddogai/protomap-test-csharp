namespace Acme.Workforce.Compliance.Attestation.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation score.</summary>
public sealed record AttestationScoreView(string Code, string Title, string RegionText, string Location, string CostText, string BatchSizeText, string DepartmentText, string StatusLabel)
{
    public static AttestationScoreView ToAttestationScoreView(AttestationScore item)
    {
        return new AttestationScoreView(
            item.Code,
            item.Title,
            item.Region,
            Text.Truncate(item.Location, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationScoreView> ToAttestationScoreViews(List<AttestationScore> items)
    {
        var views = new List<AttestationScoreView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationScoreView(item));
        }
        return views;
    }

    public static double SumAttestationScoreView(AttestationScoreView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedBatchSize;
    }
}
