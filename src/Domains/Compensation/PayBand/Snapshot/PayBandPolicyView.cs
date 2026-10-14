namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band policy.</summary>
public sealed record PayBandPolicyView(string CodeText, string Title, string CapacityText, string Approved, string Rate, string StatusLabel)
{
    public static PayBandPolicyView ToPayBandPolicyView(PayBandPolicy item)
    {
        return new PayBandPolicyView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandPolicyView> ToPayBandPolicyViews(List<PayBandPolicy> items)
    {
        var views = new List<PayBandPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandPolicyView(item));
        }
        return views;
    }

    public static double SumPayBandPolicyView(PayBandPolicyView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRate;
    }
}
