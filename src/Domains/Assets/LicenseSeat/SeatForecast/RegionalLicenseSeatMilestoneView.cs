namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using System.Globalization;

/// <summary>How screens and exports show a regional license seat milestone.</summary>
public sealed record RegionalLicenseSeatMilestoneView(string CodeText, string TitleText, string Cost, string Pinned, string EmployeeId, string Notes, string ManagerId, string Discount, string Capacity, string Version, string Skill, string StatusLabel)
{
    public static RegionalLicenseSeatMilestoneView ToRegionalLicenseSeatMilestoneView(RegionalLicenseSeatMilestone item)
    {
        return new RegionalLicenseSeatMilestoneView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.EmployeeId,
            item.Notes,
            item.ManagerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalLicenseSeatMilestoneView> ToRegionalLicenseSeatMilestoneViews(List<RegionalLicenseSeatMilestone> items)
    {
        var views = new List<RegionalLicenseSeatMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalLicenseSeatMilestoneView(item));
        }
        return views;
    }

    public static double SumRegionalLicenseSeatMilestoneView(RegionalLicenseSeatMilestoneView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedCost + parsedDiscount + (double)parsedCapacity;
    }
}
