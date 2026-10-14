namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched retro pay forecast.</summary>
public sealed record BatchedRetroPayForecastView(string Code, string Title, string EmployeeIdText, string Locale, string GradeText, string MinimumText, string Capacity, string ExternalRef, string EndsAt, string StatusLabel)
{
    public static BatchedRetroPayForecastView ToBatchedRetroPayForecastView(BatchedRetroPayForecast item)
    {
        return new BatchedRetroPayForecastView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.EmployeeId, 80),
            Text.Truncate(item.Locale, 40),
            item.Grade,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRetroPayForecastView> ToBatchedRetroPayForecastViews(List<BatchedRetroPayForecast> items)
    {
        var views = new List<BatchedRetroPayForecastView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRetroPayForecastView(item));
        }
        return views;
    }

    public static double SumBatchedRetroPayForecastView(BatchedRetroPayForecastView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedCapacity + (double)parsedEndsAt;
    }
}
