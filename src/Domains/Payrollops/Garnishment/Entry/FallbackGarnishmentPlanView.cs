namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback garnishment plan.</summary>
public sealed record FallbackGarnishmentPlanView(string Code, string Title, string GradeText, string Weight, string EndsAtText, string UpdatedAt, string Score, string StatusLabel)
{
    public static FallbackGarnishmentPlanView ToFallbackGarnishmentPlanView(FallbackGarnishmentPlan item)
    {
        return new FallbackGarnishmentPlanView(
            item.Code,
            item.Title,
            Text.Truncate(item.Grade, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackGarnishmentPlanView> ToFallbackGarnishmentPlanViews(List<FallbackGarnishmentPlan> items)
    {
        var views = new List<FallbackGarnishmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackGarnishmentPlanView(item));
        }
        return views;
    }

    public static double SumFallbackGarnishmentPlanView(FallbackGarnishmentPlanView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedEndsAt + (double)parsedUpdatedAt;
    }
}
