namespace Acme.Workforce.Engagement.Tests.Recognition.Claim;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RecognitionClaimViewChecks
{
    public static void RecognitionClaimViewShowsTheStatusInCapitals()
    {
        var item = RecognitionClaim.NewRecognitionClaim("code-755", "name-368");
        var view = RecognitionClaimView.ToRecognitionClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionClaimView.ToRecognitionClaimViews(new List<RecognitionClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionClaimViewShowsTheStatusInCapitals();
    }
}
