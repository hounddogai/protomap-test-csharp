namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledNominationExceptionViewChecks
{
    public static void ScheduledNominationExceptionViewShowsTheStatusInCapitals()
    {
        var item = ScheduledNominationException.NewScheduledNominationException("code-939", "title-290");
        var view = ScheduledNominationExceptionView.ToScheduledNominationExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledNominationExceptionView.ToScheduledNominationExceptionViews(new List<ScheduledNominationException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledNominationExceptionViewShowsTheStatusInCapitals();
    }
}
