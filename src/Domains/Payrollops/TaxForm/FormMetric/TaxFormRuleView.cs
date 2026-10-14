namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form rule.</summary>
public sealed record TaxFormRuleView(string CodeText, string Title, string LeaveDaysText, string Margin, string Region, string EndsAtText, string StatusLabel)
{
    public static TaxFormRuleView ToTaxFormRuleView(TaxFormRule item)
    {
        return new TaxFormRuleView(
            item.Code,
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormRuleView> ToTaxFormRuleViews(List<TaxFormRule> items)
    {
        var views = new List<TaxFormRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormRuleView(item));
        }
        return views;
    }

    public static double SumTaxFormRuleView(TaxFormRuleView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedMargin + (double)parsedEndsAt;
    }
}
