namespace Acme.Workforce.Talent.CareerPath.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path variance.</summary>
public sealed record CareerPathVarianceView(string Code, string Title, string Attempts, string Location, string Category, string Budget, string WindowDays, string Timezone, string StatusLabel)
{
    public static CareerPathVarianceView ToCareerPathVarianceView(CareerPathVariance item)
    {
        return new CareerPathVarianceView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Category,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathVarianceView> ToCareerPathVarianceViews(List<CareerPathVariance> items)
    {
        var views = new List<CareerPathVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathVarianceView(item));
        }
        return views;
    }

    public static double SumCareerPathVarianceView(CareerPathVarianceView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedBudget + (double)parsedWindowDays;
    }
}
