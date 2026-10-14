namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class PrimaryRecognitionVarianceViewChecks
{
    public static void PrimaryRecognitionVarianceViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("code-807", "title-942");
        var view = PrimaryRecognitionVarianceView.ToPrimaryRecognitionVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRecognitionVarianceView.ToPrimaryRecognitionVarianceViews(new List<PrimaryRecognitionVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRecognitionVarianceViewShowsTheStatusInCapitals();
    }
}
