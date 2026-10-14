namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class PendingCareerPathBatchViewChecks
{
    public static void PendingCareerPathBatchViewShowsTheStatusInCapitals()
    {
        var item = PendingCareerPathBatch.NewPendingCareerPathBatch("code-789", "title-46");
        var view = PendingCareerPathBatchView.ToPendingCareerPathBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingCareerPathBatchView.ToPendingCareerPathBatchViews(new List<PendingCareerPathBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingCareerPathBatchViewShowsTheStatusInCapitals();
    }
}
