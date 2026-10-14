namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingRecordChecks
{
    public static void OffboardingRecordStartsInRequested()
    {
        var item = OffboardingRecord.NewOffboardingRecord("code-774", "label-840");
        Check.Equal(item.Status, OffboardingRecordStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingRecordMovesToGranted()
    {
        var item = OffboardingRecord.NewOffboardingRecord("code-774", "label-840");
        try
        {
            item.MoveTo(OffboardingRecordStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingRecordStatus.Granted, "status");
    }

    public static void OffboardingRecordRejectsMovesFromExpired()
    {
        var item = OffboardingRecord.NewOffboardingRecord("code-774", "label-840");
        item.Status = OffboardingRecordStatus.Expired;
        Check.Fails(() => item.MoveTo(OffboardingRecordStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OffboardingRecordStartsInRequested();
        OffboardingRecordMovesToGranted();
        OffboardingRecordRejectsMovesFromExpired();
    }
}
