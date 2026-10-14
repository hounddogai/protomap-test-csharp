namespace Acme.Workforce.Assets.Laptop.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling laptop baseline.</summary>
public sealed record RollingLaptopBaselineView(string Code, string NameText, string Timezone, string Pinned, string LeaveDays, string Region, string MaximumText, string Notes, string Unit, string StatusLabel)
{
    public static RollingLaptopBaselineView ToRollingLaptopBaselineView(RollingLaptopBaseline item)
    {
        return new RollingLaptopBaselineView(
            item.Code,
            item.Name,
            item.Timezone,
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingLaptopBaselineView> ToRollingLaptopBaselineViews(List<RollingLaptopBaseline> items)
    {
        var views = new List<RollingLaptopBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToRollingLaptopBaselineView(item));
        }
        return views;
    }

    public static double SumRollingLaptopBaselineView(RollingLaptopBaselineView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMaximum;
    }
}
