namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PrimaryRecognitionPolicyViewChecks
{
    public static void PrimaryRecognitionPolicyViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("code-132", "label-176");
        var view = PrimaryRecognitionPolicyView.ToPrimaryRecognitionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRecognitionPolicyView.ToPrimaryRecognitionPolicyViews(new List<PrimaryRecognitionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRecognitionPolicyViewShowsTheStatusInCapitals();
    }
}
