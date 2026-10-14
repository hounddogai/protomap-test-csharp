namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueForecastViewChecks
{
    public static void TrainingDueForecastViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueForecast.NewTrainingDueForecast("code-897", "label-566");
        var view = TrainingDueForecastView.ToTrainingDueForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueForecastView.ToTrainingDueForecastViews(new List<TrainingDueForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueForecastViewShowsTheStatusInCapitals();
    }
}
