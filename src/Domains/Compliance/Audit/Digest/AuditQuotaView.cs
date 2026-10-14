namespace Acme.Workforce.Compliance.Audit.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit quota.</summary>
public sealed record AuditQuotaView(string CodeText, string NameText, string ContactEmail, string Category, string SourceText, string Version, string FteText, string Approved, string StatusLabel)
{
    public static AuditQuotaView ToAuditQuotaView(AuditQuota item)
    {
        return new AuditQuotaView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.ContactEmail, 40),
            Text.Truncate(item.Category, 40),
            item.Source,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditQuotaView> ToAuditQuotaViews(List<AuditQuota> items)
    {
        var views = new List<AuditQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToAuditQuotaView(item));
        }
        return views;
    }

    public static double SumAuditQuotaView(AuditQuotaView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedFte;
    }
}
