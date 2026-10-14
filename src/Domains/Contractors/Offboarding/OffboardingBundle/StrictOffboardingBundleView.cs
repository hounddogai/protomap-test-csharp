namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using System.Globalization;

/// <summary>How screens and exports show a strict offboarding bundle.</summary>
public sealed record StrictOffboardingBundleView(string Code, string Label, string Title, string ExpiresAt, string RevisionText, string Department, string Active, string Cost, string StatusLabel)
{
    public static StrictOffboardingBundleView ToStrictOffboardingBundleView(StrictOffboardingBundle item)
    {
        return new StrictOffboardingBundleView(
            item.Code,
            item.Label,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Active}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictOffboardingBundleView> ToStrictOffboardingBundleViews(List<StrictOffboardingBundle> items)
    {
        var views = new List<StrictOffboardingBundleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictOffboardingBundleView(item));
        }
        return views;
    }

    public static double SumStrictOffboardingBundleView(StrictOffboardingBundleView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedRevision + parsedCost;
    }
}
