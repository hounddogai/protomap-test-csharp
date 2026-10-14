namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model draft.</summary>
public sealed record CostModelDraftView(string Code, string Label, string Title, string Capacity, string Ratio, string MaximumText, string Ceiling, string EmployeeId, string StatusLabel)
{
    public static CostModelDraftView ToCostModelDraftView(CostModelDraft item)
    {
        return new CostModelDraftView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelDraftView> ToCostModelDraftViews(List<CostModelDraft> items)
    {
        var views = new List<CostModelDraftView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelDraftView(item));
        }
        return views;
    }

    public static double SumCostModelDraftView(CostModelDraftView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedRatio + parsedMaximum;
    }
}
