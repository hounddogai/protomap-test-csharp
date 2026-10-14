namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy retro pay plan.</summary>
public sealed record LegacyRetroPayPlanView(string Code, string Name, string Retries, string Source, string CapacityText, string TitleText, string SalaryBandText, string Country, string Summary, string StatusLabel)
{
    public static LegacyRetroPayPlanView ToLegacyRetroPayPlanView(LegacyRetroPayPlan item)
    {
        return new LegacyRetroPayPlanView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Title,
            Text.Truncate(item.SalaryBand, 64),
            item.Country,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRetroPayPlanView> ToLegacyRetroPayPlanViews(List<LegacyRetroPayPlan> items)
    {
        var views = new List<LegacyRetroPayPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRetroPayPlanView(item));
        }
        return views;
    }

    public static double SumLegacyRetroPayPlanView(LegacyRetroPayPlanView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedCapacity;
    }
}
