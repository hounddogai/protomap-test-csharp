namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountRequestChecks
{
    public static void HeadcountRequestStartsInPlanned()
    {
        var item = HeadcountRequest.NewHeadcountRequest("code-269", "title-321");
        Check.Equal(item.Status, HeadcountRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountRequestMovesToInProgress()
    {
        var item = HeadcountRequest.NewHeadcountRequest("code-269", "title-321");
        try
        {
            item.MoveTo(HeadcountRequestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountRequestStatus.InProgress, "status");
    }

    public static void HeadcountRequestRejectsMovesFromCancelled()
    {
        var item = HeadcountRequest.NewHeadcountRequest("code-269", "title-321");
        item.Status = HeadcountRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HeadcountRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HeadcountRequestStartsInPlanned();
        HeadcountRequestMovesToInProgress();
        HeadcountRequestRejectsMovesFromCancelled();
    }
}
