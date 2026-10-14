namespace Acme.Workforce.Talent.CareerPath.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path allocation.</summary>
public sealed record CareerPathAllocationView(string Code, string Label, string TimezoneText, string LimitText, string Weight, string Capacity, string Quantity, string DurationMinutes, string ChannelText, string Checksum, string StatusLabel)
{
    public static CareerPathAllocationView ToCareerPathAllocationView(CareerPathAllocation item)
    {
        return new CareerPathAllocationView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 40),
            item.Timezone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathAllocationView> ToCareerPathAllocationViews(List<CareerPathAllocation> items)
    {
        var views = new List<CareerPathAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathAllocationView(item));
        }
        return views;
    }

    public static double SumCareerPathAllocationView(CareerPathAllocationView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedWeight + (double)parsedCapacity;
    }
}
