namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling career path forecast.</summary>
public sealed record RollingCareerPathForecastView(string Code, string Title, string NameText, string CreatedAt, string Grade, string LeaveDays, string TenureMonthsText, string BudgetText, string StatusLabel)
{
    public static RollingCareerPathForecastView ToRollingCareerPathForecastView(RollingCareerPathForecast item)
    {
        return new RollingCareerPathForecastView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Name,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingCareerPathForecastView> ToRollingCareerPathForecastViews(List<RollingCareerPathForecast> items)
    {
        var views = new List<RollingCareerPathForecastView>();
        foreach (var item in items)
        {
            views.Add(ToRollingCareerPathForecastView(item));
        }
        return views;
    }

    public static double SumRollingCareerPathForecastView(RollingCareerPathForecastView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLeaveDays + (double)parsedTenureMonths;
    }
}
