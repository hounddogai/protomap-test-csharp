namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary attestation score.</summary>
public sealed record PrimaryAttestationScoreView(string Code, string Name, string Team, string FteText, string WeightText, string Approved, string VersionText, string Category, string Title, string DescriptionText, string StatusLabel)
{
    public static PrimaryAttestationScoreView ToPrimaryAttestationScoreView(PrimaryAttestationScore item)
    {
        return new PrimaryAttestationScoreView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Team, 120),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Title,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAttestationScoreView> ToPrimaryAttestationScoreViews(List<PrimaryAttestationScore> items)
    {
        var views = new List<PrimaryAttestationScoreView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAttestationScoreView(item));
        }
        return views;
    }

    public static double SumPrimaryAttestationScoreView(PrimaryAttestationScoreView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return parsedFte + parsedWeight + (double)parsedVersion;
    }
}
