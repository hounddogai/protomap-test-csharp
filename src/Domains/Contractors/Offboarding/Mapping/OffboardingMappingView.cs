namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding mapping.</summary>
public sealed record OffboardingMappingView(string Code, string Title, string BatchSize, string Category, string Locale, string Ratio, string Maximum, string OwnerIdText, string Attempts, string StatusLabel)
{
    public static OffboardingMappingView ToOffboardingMappingView(OffboardingMapping item)
    {
        return new OffboardingMappingView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Locale,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingMappingView> ToOffboardingMappingViews(List<OffboardingMapping> items)
    {
        var views = new List<OffboardingMappingView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingMappingView(item));
        }
        return views;
    }

    public static double SumOffboardingMappingView(OffboardingMappingView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedRatio + parsedMaximum;
    }
}
