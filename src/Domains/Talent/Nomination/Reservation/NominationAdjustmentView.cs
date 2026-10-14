namespace Acme.Workforce.Talent.Nomination.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination adjustment.</summary>
public sealed record NominationAdjustmentView(string Code, string TitleText, string RegionText, string Phone, string DiscountText, string NameText, string WeightText, string Segment, string Category, string StatusLabel)
{
    public static NominationAdjustmentView ToNominationAdjustmentView(NominationAdjustment item)
    {
        return new NominationAdjustmentView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Region, 80),
            item.Phone,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            Text.Truncate(item.Category, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationAdjustmentView> ToNominationAdjustmentViews(List<NominationAdjustment> items)
    {
        var views = new List<NominationAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToNominationAdjustmentView(item));
        }
        return views;
    }

    public static double SumNominationAdjustmentView(NominationAdjustmentView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedWeight;
    }
}
