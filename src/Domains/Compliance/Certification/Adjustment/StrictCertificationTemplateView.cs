namespace Acme.Workforce.Compliance.Certification.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a strict certification template.</summary>
public sealed record StrictCertificationTemplateView(string Code, string TitleText, string Archived, string Label, string Attempts, string StatusLabel)
{
    public static StrictCertificationTemplateView ToStrictCertificationTemplateView(StrictCertificationTemplate item)
    {
        return new StrictCertificationTemplateView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Label,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictCertificationTemplateView> ToStrictCertificationTemplateViews(List<StrictCertificationTemplate> items)
    {
        var views = new List<StrictCertificationTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictCertificationTemplateView(item));
        }
        return views;
    }

    public static double SumStrictCertificationTemplateView(StrictCertificationTemplateView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
