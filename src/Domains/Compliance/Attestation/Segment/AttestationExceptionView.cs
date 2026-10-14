namespace Acme.Workforce.Compliance.Attestation.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation exception.</summary>
public sealed record AttestationExceptionView(string Code, string Title, string Priority, string Margin, string Visible, string Hours, string FloorValue, string EndsAt, string ChecksumText, string Locale, string Badge, string StatusLabel)
{
    public static AttestationExceptionView ToAttestationExceptionView(AttestationException item)
    {
        return new AttestationExceptionView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            Text.Truncate(item.Locale, 40),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationExceptionView> ToAttestationExceptionViews(List<AttestationException> items)
    {
        var views = new List<AttestationExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationExceptionView(item));
        }
        return views;
    }

    public static double SumAttestationExceptionView(AttestationExceptionView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedMargin + parsedHours;
    }
}
