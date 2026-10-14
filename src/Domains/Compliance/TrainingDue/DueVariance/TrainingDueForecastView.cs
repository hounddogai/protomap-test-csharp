namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using System.Globalization;

/// <summary>How screens and exports show a training due forecast.</summary>
public sealed record TrainingDueForecastView(string Code, string Label, string Approved, string Rate, string Archived, string Skill, string Hours, string UpdatedAt, string Weight, string Budget, string StatusLabel)
{
    public static TrainingDueForecastView ToTrainingDueForecastView(TrainingDueForecast item)
    {
        return new TrainingDueForecastView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Skill,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueForecastView> ToTrainingDueForecastViews(List<TrainingDueForecast> items)
    {
        var views = new List<TrainingDueForecastView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueForecastView(item));
        }
        return views;
    }

    public static double SumTrainingDueForecastView(TrainingDueForecastView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRate + parsedHours + (double)parsedUpdatedAt;
    }
}
