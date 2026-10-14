namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPayRunTargetChecks
{
    public static void LegacyPayRunTargetStartsInCreated()
    {
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("code-587", "name-357");
        Check.Equal(item.Status, LegacyPayRunTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayRunTargetMovesToRetired()
    {
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("code-587", "name-357");
        try
        {
            item.MoveTo(LegacyPayRunTargetStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayRunTargetStatus.Retired, "status");
    }

    public static void LegacyPayRunTargetRejectsMovesFromRetired()
    {
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("code-587", "name-357");
        item.Status = LegacyPayRunTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyPayRunTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyPayRunTargetStartsInCreated();
        LegacyPayRunTargetMovesToRetired();
        LegacyPayRunTargetRejectsMovesFromRetired();
    }
}
