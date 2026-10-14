namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary reorg plan.</summary>
public sealed record PrimaryReorgPlanView(string Code, string LabelText, string Title, string LocationText, string Cost, string Badge, string StartsAt, string StatusLabel)
{
    public static PrimaryReorgPlanView ToPrimaryReorgPlanView(PrimaryReorgPlan item)
    {
        return new PrimaryReorgPlanView(
            item.Code,
            item.Label,
            item.Title,
            item.Location,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryReorgPlanView> ToPrimaryReorgPlanViews(List<PrimaryReorgPlan> items)
    {
        var views = new List<PrimaryReorgPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryReorgPlanView(item));
        }
        return views;
    }

    public static double SumPrimaryReorgPlanView(PrimaryReorgPlanView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedStartsAt;
    }
}
