namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointViewChecks
{
    public static void HeadcountCheckpointViewShowsTheStatusInCapitals()
    {
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
        var view = HeadcountCheckpointView.ToHeadcountCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountCheckpointView.ToHeadcountCheckpointViews(new List<HeadcountCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountCheckpointViewShowsTheStatusInCapitals();
    }
}
