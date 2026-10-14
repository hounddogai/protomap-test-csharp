namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional audit draft.</summary>
public sealed record RegionalAuditDraftView(string Code, string Label, string CreatedAt, string LocaleText, string ChannelText, string ContactEmail, string FteText, string StatusLabel)
{
    public static RegionalAuditDraftView ToRegionalAuditDraftView(RegionalAuditDraft item)
    {
        return new RegionalAuditDraftView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 40),
            item.Channel,
            item.ContactEmail,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAuditDraftView> ToRegionalAuditDraftViews(List<RegionalAuditDraft> items)
    {
        var views = new List<RegionalAuditDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAuditDraftView(item));
        }
        return views;
    }

    public static double SumRegionalAuditDraftView(RegionalAuditDraftView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedFte;
    }
}
