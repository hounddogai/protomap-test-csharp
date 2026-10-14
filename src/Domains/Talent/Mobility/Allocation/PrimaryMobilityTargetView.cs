namespace Acme.Workforce.Talent.Mobility.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary mobility target.</summary>
public sealed record PrimaryMobilityTargetView(string Code, string Title, string DescriptionText, string Channel, string Country, string Minimum, string JobCodeText, string CapacityText, string StatusLabel)
{
    public static PrimaryMobilityTargetView ToPrimaryMobilityTargetView(PrimaryMobilityTarget item)
    {
        return new PrimaryMobilityTargetView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Description,
            item.Channel,
            Text.Truncate(item.Country, 64),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryMobilityTargetView> ToPrimaryMobilityTargetViews(List<PrimaryMobilityTarget> items)
    {
        var views = new List<PrimaryMobilityTargetView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryMobilityTargetView(item));
        }
        return views;
    }

    public static double SumPrimaryMobilityTargetView(PrimaryMobilityTargetView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCapacity;
    }
}
