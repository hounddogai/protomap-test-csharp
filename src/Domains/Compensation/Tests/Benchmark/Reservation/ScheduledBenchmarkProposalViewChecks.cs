namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledBenchmarkProposalViewChecks
{
    public static void ScheduledBenchmarkProposalViewShowsTheStatusInCapitals()
    {
        var item = ScheduledBenchmarkProposal.NewScheduledBenchmarkProposal("code-743", "title-987");
        var view = ScheduledBenchmarkProposalView.ToScheduledBenchmarkProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledBenchmarkProposalView.ToScheduledBenchmarkProposalViews(new List<ScheduledBenchmarkProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkProposalViewShowsTheStatusInCapitals();
    }
}
