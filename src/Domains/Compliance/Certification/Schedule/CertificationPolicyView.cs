namespace Acme.Workforce.Compliance.Certification.Schedule;

using System.Globalization;

/// <summary>How screens and exports show a certification policy.</summary>
public sealed record CertificationPolicyView(string CodeText, string NameText, string Version, string Location, string PriorityText, string StatusLabel)
{
    public static CertificationPolicyView ToCertificationPolicyView(CertificationPolicy item)
    {
        return new CertificationPolicyView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationPolicyView> ToCertificationPolicyViews(List<CertificationPolicy> items)
    {
        var views = new List<CertificationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationPolicyView(item));
        }
        return views;
    }

    public static double SumCertificationPolicyView(CertificationPolicyView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedPriority;
    }
}
