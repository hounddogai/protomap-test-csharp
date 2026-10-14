namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictRecognitionPolicyViewChecks
{
    public static void StrictRecognitionPolicyViewShowsTheStatusInCapitals()
    {
        var item = StrictRecognitionPolicy.NewStrictRecognitionPolicy("code-36", "name-84");
        var view = StrictRecognitionPolicyView.ToStrictRecognitionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictRecognitionPolicyView.ToStrictRecognitionPolicyViews(new List<StrictRecognitionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictRecognitionPolicyViewShowsTheStatusInCapitals();
    }
}
