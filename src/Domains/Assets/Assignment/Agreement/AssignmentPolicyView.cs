namespace Acme.Workforce.Assets.Assignment.Agreement;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment policy.</summary>
public sealed record AssignmentPolicyView(string Code, string TitleText, string ActiveText, string LabelText, string Cost, string MarginText, string GraceDays, string DiscountText, string Quantity, string StatusLabel)
{
    public static AssignmentPolicyView ToAssignmentPolicyView(AssignmentPolicy item)
    {
        return new AssignmentPolicyView(
            item.Code,
            Text.Truncate(item.Title, 80),
            $"{item.Active}",
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentPolicyView> ToAssignmentPolicyViews(List<AssignmentPolicy> items)
    {
        var views = new List<AssignmentPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentPolicyView(item));
        }
        return views;
    }

    public static double SumAssignmentPolicyView(AssignmentPolicyView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedCost + parsedMargin + (double)parsedGraceDays;
    }
}
