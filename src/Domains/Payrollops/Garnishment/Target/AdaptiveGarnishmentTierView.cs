namespace Acme.Workforce.Payrollops.Garnishment.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive garnishment tier.</summary>
public sealed record AdaptiveGarnishmentTierView(string CodeText, string NameText, string Timezone, string MaximumText, string WeightText, string Checksum, string EmployeeIdText, string GraceDays, string Limit, string Approved, string StatusLabel)
{
    public static AdaptiveGarnishmentTierView ToAdaptiveGarnishmentTierView(AdaptiveGarnishmentTier item)
    {
        return new AdaptiveGarnishmentTierView(
            item.Code,
            item.Name,
            item.Timezone,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.EmployeeId, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveGarnishmentTierView> ToAdaptiveGarnishmentTierViews(List<AdaptiveGarnishmentTier> items)
    {
        var views = new List<AdaptiveGarnishmentTierView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveGarnishmentTierView(item));
        }
        return views;
    }

    public static double SumAdaptiveGarnishmentTierView(AdaptiveGarnishmentTierView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedWeight + (double)parsedGraceDays;
    }
}
