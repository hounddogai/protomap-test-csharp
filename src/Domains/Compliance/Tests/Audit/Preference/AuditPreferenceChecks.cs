namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceChecks
{
    public static void AuditPreferenceStartsInNew()
    {
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
        Check.Equal(item.Status, AuditPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditPreferenceMovesToRunning()
    {
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
        try
        {
            item.MoveTo(AuditPreferenceStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditPreferenceStatus.Running, "status");
    }

    public static void AuditPreferenceRejectsMovesFromFailed()
    {
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
        item.Status = AuditPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(AuditPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        AuditPreferenceStartsInNew();
        AuditPreferenceMovesToRunning();
        AuditPreferenceRejectsMovesFromFailed();
    }
}
