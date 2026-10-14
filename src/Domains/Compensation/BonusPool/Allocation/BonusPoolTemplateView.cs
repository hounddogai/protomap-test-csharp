namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool template.</summary>
public sealed record BonusPoolTemplateView(string Code, string Title, string OwnerId, string Department, string TeamText, string LabelText, string CostCenter, string QuantityText, string StatusLabel)
{
    public static BonusPoolTemplateView ToBonusPoolTemplateView(BonusPoolTemplate item)
    {
        return new BonusPoolTemplateView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.OwnerId,
            item.Department,
            item.Team,
            Text.Truncate(item.Label, 80),
            item.CostCenter,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolTemplateView> ToBonusPoolTemplateViews(List<BonusPoolTemplate> items)
    {
        var views = new List<BonusPoolTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolTemplateView(item));
        }
        return views;
    }

    public static double SumBonusPoolTemplateView(BonusPoolTemplateView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
