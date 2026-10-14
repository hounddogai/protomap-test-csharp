namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation group.</summary>
public sealed record AttestationGroupView(string Code, string NameText, string FloorValue, string Phone, string EndsAt, string StatusLabel)
{
    public static AttestationGroupView ToAttestationGroupView(AttestationGroup item)
    {
        return new AttestationGroupView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationGroupView> ToAttestationGroupViews(List<AttestationGroup> items)
    {
        var views = new List<AttestationGroupView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationGroupView(item));
        }
        return views;
    }

    public static double SumAttestationGroupView(AttestationGroupView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedEndsAt;
    }
}
