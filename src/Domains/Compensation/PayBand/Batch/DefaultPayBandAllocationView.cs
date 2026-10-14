namespace Acme.Workforce.Compensation.PayBand.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pay band allocation.</summary>
public sealed record DefaultPayBandAllocationView(string CodeText, string NameText, string CategoryText, string Region, string Maximum, string Department, string Version, string StatusLabel)
{
    public static DefaultPayBandAllocationView ToDefaultPayBandAllocationView(DefaultPayBandAllocation item)
    {
        return new DefaultPayBandAllocationView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Category,
            item.Region,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayBandAllocationView> ToDefaultPayBandAllocationViews(List<DefaultPayBandAllocation> items)
    {
        var views = new List<DefaultPayBandAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayBandAllocationView(item));
        }
        return views;
    }

    public static double SumDefaultPayBandAllocationView(DefaultPayBandAllocationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedVersion;
    }
}
