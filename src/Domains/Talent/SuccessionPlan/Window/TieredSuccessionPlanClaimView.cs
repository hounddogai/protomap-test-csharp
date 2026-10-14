namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered succession plan claim.</summary>
public sealed record TieredSuccessionPlanClaimView(string Code, string TitleText, string Phone, string Category, string UpdatedAt, string OwnerIdText, string StatusLabel)
{
    public static TieredSuccessionPlanClaimView ToTieredSuccessionPlanClaimView(TieredSuccessionPlanClaim item)
    {
        return new TieredSuccessionPlanClaimView(
            item.Code,
            item.Title,
            item.Phone,
            item.Category,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSuccessionPlanClaimView> ToTieredSuccessionPlanClaimViews(List<TieredSuccessionPlanClaim> items)
    {
        var views = new List<TieredSuccessionPlanClaimView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSuccessionPlanClaimView(item));
        }
        return views;
    }

    public static double SumTieredSuccessionPlanClaimView(TieredSuccessionPlanClaimView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
