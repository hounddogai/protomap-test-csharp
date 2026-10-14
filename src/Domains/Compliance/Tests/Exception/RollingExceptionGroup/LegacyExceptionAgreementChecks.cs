namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class LegacyExceptionAgreementChecks
{
    public static void LegacyExceptionAgreementStartsInIdle()
    {
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("code-202", "name-227");
        Check.Equal(item.Status, LegacyExceptionAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyExceptionAgreementMovesToWarming()
    {
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("code-202", "name-227");
        try
        {
            item.MoveTo(LegacyExceptionAgreementStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyExceptionAgreementStatus.Warming, "status");
    }

    public static void LegacyExceptionAgreementRejectsMovesFromStopped()
    {
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("code-202", "name-227");
        item.Status = LegacyExceptionAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyExceptionAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyExceptionAgreementStartsInIdle();
        LegacyExceptionAgreementMovesToWarming();
        LegacyExceptionAgreementRejectsMovesFromStopped();
    }
}
