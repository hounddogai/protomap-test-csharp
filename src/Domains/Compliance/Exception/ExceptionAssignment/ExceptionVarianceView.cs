namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception variance.</summary>
public sealed record ExceptionVarianceView(string Code, string Title, string Description, string Discount, string Currency, string Location, string SkillText, string Limit, string Retries, string StatusLabel)
{
    public static ExceptionVarianceView ToExceptionVarianceView(ExceptionVariance item)
    {
        return new ExceptionVarianceView(
            item.Code,
            item.Title,
            Text.Truncate(item.Description, 40),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            Text.Truncate(item.Location, 40),
            item.Skill,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionVarianceView> ToExceptionVarianceViews(List<ExceptionVariance> items)
    {
        var views = new List<ExceptionVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionVarianceView(item));
        }
        return views;
    }

    public static double SumExceptionVarianceView(ExceptionVarianceView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit + (double)parsedRetries;
    }
}
