namespace Acme.Workforce.Talent.CareerPath.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path adjustment.</summary>
public sealed record CareerPathAdjustmentView(string Code, string TitleText, string CostCenter, string ManagerId, string DiscountText, string LabelText, string OwnerId, string Channel, string StatusLabel)
{
    public static CareerPathAdjustmentView ToCareerPathAdjustmentView(CareerPathAdjustment item)
    {
        return new CareerPathAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.CostCenter,
            item.ManagerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.OwnerId, 64),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathAdjustmentView> ToCareerPathAdjustmentViews(List<CareerPathAdjustment> items)
    {
        var views = new List<CareerPathAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathAdjustmentView(item));
        }
        return views;
    }

    public static double SumCareerPathAdjustmentView(CareerPathAdjustmentView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
