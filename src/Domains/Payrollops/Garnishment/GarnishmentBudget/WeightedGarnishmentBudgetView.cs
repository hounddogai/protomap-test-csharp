namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using System.Globalization;

/// <summary>How screens and exports show a weighted garnishment budget.</summary>
public sealed record WeightedGarnishmentBudgetView(string Code, string Name, string Label, string Score, string ApprovedText, string GraceDaysText, string JobCodeText, string Timezone, string Amount, string SequenceText, string TenureMonthsText, string StatusLabel)
{
    public static WeightedGarnishmentBudgetView ToWeightedGarnishmentBudgetView(WeightedGarnishmentBudget item)
    {
        return new WeightedGarnishmentBudgetView(
            item.Code,
            item.Name,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Timezone,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedGarnishmentBudgetView> ToWeightedGarnishmentBudgetViews(List<WeightedGarnishmentBudget> items)
    {
        var views = new List<WeightedGarnishmentBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedGarnishmentBudgetView(item));
        }
        return views;
    }

    public static double SumWeightedGarnishmentBudgetView(WeightedGarnishmentBudgetView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedGraceDays + parsedAmount;
    }
}
