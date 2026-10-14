namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model group.</summary>
public sealed record CostModelGroupView(string Code, string NameText, string Ceiling, string PriorityText, string TenureMonths, string Maximum, string StatusLabel)
{
    public static CostModelGroupView ToCostModelGroupView(CostModelGroup item)
    {
        return new CostModelGroupView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 40),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelGroupView> ToCostModelGroupViews(List<CostModelGroup> items)
    {
        var views = new List<CostModelGroupView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelGroupView(item));
        }
        return views;
    }

    public static double SumCostModelGroupView(CostModelGroupView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedPriority + (double)parsedTenureMonths;
    }
}
