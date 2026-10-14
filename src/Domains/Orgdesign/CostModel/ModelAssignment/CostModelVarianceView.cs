namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model variance.</summary>
public sealed record CostModelVarianceView(string Code, string NameText, string MinimumText, string Skill, string Unit, string StatusLabel)
{
    public static CostModelVarianceView ToCostModelVarianceView(CostModelVariance item)
    {
        return new CostModelVarianceView(
            item.Code,
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            Text.Truncate(item.Unit, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelVarianceView> ToCostModelVarianceViews(List<CostModelVariance> items)
    {
        var views = new List<CostModelVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelVarianceView(item));
        }
        return views;
    }

    public static double SumCostModelVarianceView(CostModelVarianceView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
