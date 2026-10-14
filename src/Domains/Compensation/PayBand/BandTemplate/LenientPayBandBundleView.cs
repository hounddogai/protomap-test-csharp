namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient pay band bundle.</summary>
public sealed record LenientPayBandBundleView(string Code, string Label, string ExpiresAt, string JobCode, string RevisionText, string EmployeeId, string Category, string Phone, string Version, string Name, string StatusLabel)
{
    public static LenientPayBandBundleView ToLenientPayBandBundleView(LenientPayBandBundle item)
    {
        return new LenientPayBandBundleView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Category,
            Text.Truncate(item.Phone, 80),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayBandBundleView> ToLenientPayBandBundleViews(List<LenientPayBandBundle> items)
    {
        var views = new List<LenientPayBandBundleView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayBandBundleView(item));
        }
        return views;
    }

    public static double SumLenientPayBandBundleView(LenientPayBandBundleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedRevision + (double)parsedVersion;
    }
}
