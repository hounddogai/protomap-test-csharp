namespace Acme.Workforce.Compliance.Exception.Audit;

using System.Globalization;

/// <summary>How screens and exports show a regional exception audit.</summary>
public sealed record RegionalExceptionAuditView(string CodeText, string NameText, string ExpiresAtText, string Rate, string Label, string StatusLabel)
{
    public static RegionalExceptionAuditView ToRegionalExceptionAuditView(RegionalExceptionAudit item)
    {
        return new RegionalExceptionAuditView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalExceptionAuditView> ToRegionalExceptionAuditViews(List<RegionalExceptionAudit> items)
    {
        var views = new List<RegionalExceptionAuditView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalExceptionAuditView(item));
        }
        return views;
    }

    public static double SumRegionalExceptionAuditView(RegionalExceptionAuditView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedRate;
    }
}
