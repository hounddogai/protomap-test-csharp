namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class TieredOffboardingRecordChecks
{
    public static void TieredOffboardingRecordStartsInPlanned()
    {
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("code-33", "name-610");
        Check.Equal(item.Status, TieredOffboardingRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredOffboardingRecordMovesToCompleted()
    {
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("code-33", "name-610");
        try
        {
            item.MoveTo(TieredOffboardingRecordStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredOffboardingRecordStatus.Completed, "status");
    }

    public static void TieredOffboardingRecordRejectsMovesFromCancelled()
    {
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("code-33", "name-610");
        item.Status = TieredOffboardingRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredOffboardingRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredOffboardingRecordStartsInPlanned();
        TieredOffboardingRecordMovesToCompleted();
        TieredOffboardingRecordRejectsMovesFromCancelled();
    }
}
