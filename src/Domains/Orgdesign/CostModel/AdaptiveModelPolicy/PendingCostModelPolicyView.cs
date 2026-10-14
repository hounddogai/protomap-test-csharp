namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending cost model policy.</summary>
public sealed record PendingCostModelPolicyView(string CodeText, string LabelText, string Archived, string MinimumText, string EndsAt, string Fte, string ChannelText, string Name, string RatingText, string Source, string Region, string StatusLabel)
{
    public static PendingCostModelPolicyView ToPendingCostModelPolicyView(PendingCostModelPolicy item)
    {
        return new PendingCostModelPolicyView(
            Text.Truncate(item.Code, 80),
            item.Label,
            $"{item.Archived}",
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 80),
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            Text.Truncate(item.Region, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingCostModelPolicyView> ToPendingCostModelPolicyViews(List<PendingCostModelPolicy> items)
    {
        var views = new List<PendingCostModelPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPendingCostModelPolicyView(item));
        }
        return views;
    }

    public static double SumPendingCostModelPolicyView(PendingCostModelPolicyView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedEndsAt + parsedFte;
    }
}
