namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class PrimaryRecognitionVarianceChecks
{
    public static void PrimaryRecognitionVarianceStartsInCreated()
    {
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("code-807", "title-942");
        Check.Equal(item.Status, PrimaryRecognitionVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRecognitionVarianceMovesToPublished()
    {
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("code-807", "title-942");
        try
        {
            item.MoveTo(PrimaryRecognitionVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRecognitionVarianceStatus.Published, "status");
    }

    public static void PrimaryRecognitionVarianceRejectsMovesFromRetired()
    {
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("code-807", "title-942");
        item.Status = PrimaryRecognitionVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryRecognitionVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryRecognitionVarianceStartsInCreated();
        PrimaryRecognitionVarianceMovesToPublished();
        PrimaryRecognitionVarianceRejectsMovesFromRetired();
    }
}
