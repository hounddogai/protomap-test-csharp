namespace Acme.Workforce.Compliance.Attestation.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation allocation.</summary>
public sealed record AttestationAllocationView(string CodeText, string TitleText, string MinimumText, string ScoreText, string LocationText, string ContactEmail, string CreatedAtText, string Fte, string StatusLabel)
{
    public static AttestationAllocationView ToAttestationAllocationView(AttestationAllocation item)
    {
        return new AttestationAllocationView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            item.ContactEmail,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationAllocationView> ToAttestationAllocationViews(List<AttestationAllocation> items)
    {
        var views = new List<AttestationAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationAllocationView(item));
        }
        return views;
    }

    public static double SumAttestationAllocationView(AttestationAllocationView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedScore + (double)parsedCreatedAt;
    }
}
