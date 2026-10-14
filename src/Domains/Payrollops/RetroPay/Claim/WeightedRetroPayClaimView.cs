namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted retro pay claim.</summary>
public sealed record WeightedRetroPayClaimView(string Code, string Title, string Minimum, string ManagerId, string Locale, string Currency, string ContactEmail, string Capacity, string StatusLabel)
{
    public static WeightedRetroPayClaimView ToWeightedRetroPayClaimView(WeightedRetroPayClaim item)
    {
        return new WeightedRetroPayClaimView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Locale,
            item.Currency,
            Text.Truncate(item.ContactEmail, 40),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedRetroPayClaimView> ToWeightedRetroPayClaimViews(List<WeightedRetroPayClaim> items)
    {
        var views = new List<WeightedRetroPayClaimView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedRetroPayClaimView(item));
        }
        return views;
    }

    public static double SumWeightedRetroPayClaimView(WeightedRetroPayClaimView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCapacity;
    }
}
