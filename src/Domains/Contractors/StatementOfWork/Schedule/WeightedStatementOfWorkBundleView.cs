namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted statement of work bundle.</summary>
public sealed record WeightedStatementOfWorkBundleView(string Code, string NameText, string Grade, string UnitText, string Phone, string CurrencyText, string DurationMinutesText, string LimitText, string StatusLabel)
{
    public static WeightedStatementOfWorkBundleView ToWeightedStatementOfWorkBundleView(WeightedStatementOfWorkBundle item)
    {
        return new WeightedStatementOfWorkBundleView(
            Text.Truncate(item.Code, 80),
            item.Name,
            Text.Truncate(item.Grade, 120),
            Text.Truncate(item.Unit, 120),
            item.Phone,
            item.Currency,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedStatementOfWorkBundleView> ToWeightedStatementOfWorkBundleViews(List<WeightedStatementOfWorkBundle> items)
    {
        var views = new List<WeightedStatementOfWorkBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedStatementOfWorkBundleView(item));
        }
        return views;
    }

    public static double SumWeightedStatementOfWorkBundleView(WeightedStatementOfWorkBundleView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedLimit;
    }
}
