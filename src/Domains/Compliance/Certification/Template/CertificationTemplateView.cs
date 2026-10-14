namespace Acme.Workforce.Compliance.Certification.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification template.</summary>
public sealed record CertificationTemplateView(string Code, string LabelText, string RatioText, string ManagerIdText, string HoursText, string Weight, string StatusLabel)
{
    public static CertificationTemplateView ToCertificationTemplateView(CertificationTemplate item)
    {
        return new CertificationTemplateView(
            item.Code,
            item.Label,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationTemplateView> ToCertificationTemplateViews(List<CertificationTemplate> items)
    {
        var views = new List<CertificationTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationTemplateView(item));
        }
        return views;
    }

    public static double SumCertificationTemplateView(CertificationTemplateView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedRatio + parsedHours + parsedWeight;
    }
}
