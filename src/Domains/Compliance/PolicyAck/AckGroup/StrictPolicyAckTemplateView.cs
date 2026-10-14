namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict policy ack template.</summary>
public sealed record StrictPolicyAckTemplateView(string Code, string Label, string Rating, string UnitText, string Ratio, string Name, string VisibleText, string ManagerId, string CostText, string StatusLabel)
{
    public static StrictPolicyAckTemplateView ToStrictPolicyAckTemplateView(StrictPolicyAckTemplate item)
    {
        return new StrictPolicyAckTemplateView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Visible}",
            item.ManagerId,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyAckTemplateView> ToStrictPolicyAckTemplateViews(List<StrictPolicyAckTemplate> items)
    {
        var views = new List<StrictPolicyAckTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyAckTemplateView(item));
        }
        return views;
    }

    public static double SumStrictPolicyAckTemplateView(StrictPolicyAckTemplateView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedRating + parsedRatio + parsedCost;
    }
}
