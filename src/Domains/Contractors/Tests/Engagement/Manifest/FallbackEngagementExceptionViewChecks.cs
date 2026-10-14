namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackEngagementExceptionViewChecks
{
    public static void FallbackEngagementExceptionViewShowsTheStatusInCapitals()
    {
        var item = FallbackEngagementException.NewFallbackEngagementException("code-508", "label-82");
        var view = FallbackEngagementExceptionView.ToFallbackEngagementExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackEngagementExceptionView.ToFallbackEngagementExceptionViews(new List<FallbackEngagementException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackEngagementExceptionViewShowsTheStatusInCapitals();
    }
}
