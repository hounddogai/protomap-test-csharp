namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted license seat estimate.</summary>
public sealed record WeightedLicenseSeatEstimateView(string Code, string Name, string Capacity, string ContactEmailText, string UpdatedAtText, string SalaryBand, string TimezoneText, string ManagerId, string StatusLabel)
{
    public static WeightedLicenseSeatEstimateView ToWeightedLicenseSeatEstimateView(WeightedLicenseSeatEstimate item)
    {
        return new WeightedLicenseSeatEstimateView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Timezone,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedLicenseSeatEstimateView> ToWeightedLicenseSeatEstimateViews(List<WeightedLicenseSeatEstimate> items)
    {
        var views = new List<WeightedLicenseSeatEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedLicenseSeatEstimateView(item));
        }
        return views;
    }

    public static double SumWeightedLicenseSeatEstimateView(WeightedLicenseSeatEstimateView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedUpdatedAt;
    }
}
