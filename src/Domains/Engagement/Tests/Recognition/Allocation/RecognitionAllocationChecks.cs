namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionAllocationChecks
{
    public static void RecognitionAllocationStartsInRequested()
    {
        var item = RecognitionAllocation.NewRecognitionAllocation("code-876", "label-614");
        Check.Equal(item.Status, RecognitionAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionAllocationMovesToGranted()
    {
        var item = RecognitionAllocation.NewRecognitionAllocation("code-876", "label-614");
        try
        {
            item.MoveTo(RecognitionAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionAllocationStatus.Granted, "status");
    }

    public static void RecognitionAllocationRejectsMovesFromExpired()
    {
        var item = RecognitionAllocation.NewRecognitionAllocation("code-876", "label-614");
        item.Status = RecognitionAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(RecognitionAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RecognitionAllocationStartsInRequested();
        RecognitionAllocationMovesToGranted();
        RecognitionAllocationRejectsMovesFromExpired();
    }
}
