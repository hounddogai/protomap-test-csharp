namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default attestation rollup.</summary>
public sealed record DefaultAttestationRollupView(string Code, string Label, string FteText, string ExpiresAtText, string JobCodeText, string StatusLabel)
{
    public static DefaultAttestationRollupView ToDefaultAttestationRollupView(DefaultAttestationRollup item)
    {
        return new DefaultAttestationRollupView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAttestationRollupView> ToDefaultAttestationRollupViews(List<DefaultAttestationRollup> items)
    {
        var views = new List<DefaultAttestationRollupView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAttestationRollupView(item));
        }
        return views;
    }

    public static double SumDefaultAttestationRollupView(DefaultAttestationRollupView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedExpiresAt;
    }
}
