namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathDraftChecks
{
    public static void CareerPathDraftStartsInPlanned()
    {
        var item = CareerPathDraft.NewCareerPathDraft("code-538", "label-879");
        Check.Equal(item.Status, CareerPathDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathDraftMovesToCancelled()
    {
        var item = CareerPathDraft.NewCareerPathDraft("code-538", "label-879");
        try
        {
            item.MoveTo(CareerPathDraftStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathDraftStatus.Cancelled, "status");
    }

    public static void CareerPathDraftRejectsMovesFromCancelled()
    {
        var item = CareerPathDraft.NewCareerPathDraft("code-538", "label-879");
        item.Status = CareerPathDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathDraftStartsInPlanned();
        CareerPathDraftMovesToCancelled();
        CareerPathDraftRejectsMovesFromCancelled();
    }
}
