namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling cost model batch.</summary>
public sealed record RollingCostModelBatchView(string Code, string Label, string SkillText, string Team, string Currency, string CreatedAt, string StatusLabel)
{
    public static RollingCostModelBatchView ToRollingCostModelBatchView(RollingCostModelBatch item)
    {
        return new RollingCostModelBatchView(
            Text.Truncate(item.Code, 80),
            item.Label,
            Text.Truncate(item.Skill, 64),
            item.Team,
            item.Currency,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingCostModelBatchView> ToRollingCostModelBatchViews(List<RollingCostModelBatch> items)
    {
        var views = new List<RollingCostModelBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRollingCostModelBatchView(item));
        }
        return views;
    }

    public static double SumRollingCostModelBatchView(RollingCostModelBatchView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
