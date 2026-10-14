namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict reorg budget.</summary>
public sealed record StrictReorgBudgetView(string Code, string NameText, string Checksum, string LimitText, string EndsAt, string Channel, string LocaleText, string SalaryBandText, string StatusLabel)
{
    public static StrictReorgBudgetView ToStrictReorgBudgetView(StrictReorgBudget item)
    {
        return new StrictReorgBudgetView(
            item.Code,
            item.Name,
            Text.Truncate(item.Checksum, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Channel,
            item.Locale,
            Text.Truncate(item.SalaryBand, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictReorgBudgetView> ToStrictReorgBudgetViews(List<StrictReorgBudget> items)
    {
        var views = new List<StrictReorgBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToStrictReorgBudgetView(item));
        }
        return views;
    }

    public static double SumStrictReorgBudgetView(StrictReorgBudgetView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedEndsAt;
    }
}
