namespace Acme.Workforce.Orgdesign.CostModel.Group;

using System.Globalization;

/// <summary>How screens and exports show a cost model tier.</summary>
public sealed record CostModelTierView(string Code, string Name, string Channel, string Fte, string ApprovedText, string StatusLabel)
{
    public static CostModelTierView ToCostModelTierView(CostModelTier item)
    {
        return new CostModelTierView(
            item.Code,
            item.Name,
            item.Channel,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelTierView> ToCostModelTierViews(List<CostModelTier> items)
    {
        var views = new List<CostModelTierView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelTierView(item));
        }
        return views;
    }

    public static double SumCostModelTierView(CostModelTierView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
