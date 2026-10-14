namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop override.</summary>
public sealed record LaptopOverrideView(string Code, string Title, string Threshold, string Version, string EmployeeId, string Revision, string StatusLabel)
{
    public static LaptopOverrideView ToLaptopOverrideView(LaptopOverride item)
    {
        return new LaptopOverrideView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopOverrideView> ToLaptopOverrideViews(List<LaptopOverride> items)
    {
        var views = new List<LaptopOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopOverrideView(item));
        }
        return views;
    }

    public static double SumLaptopOverrideView(LaptopOverrideView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedVersion + (double)parsedRevision;
    }
}
