namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ReorgAuditChecks
{
    public static void ReorgAuditStartsInNew()
    {
        var item = ReorgAudit.NewReorgAudit("code-806", "name-499");
        Check.Equal(item.Status, ReorgAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgAuditMovesToFailed()
    {
        var item = ReorgAudit.NewReorgAudit("code-806", "name-499");
        try
        {
            item.MoveTo(ReorgAuditStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgAuditStatus.Failed, "status");
    }

    public static void ReorgAuditRejectsMovesFromFailed()
    {
        var item = ReorgAudit.NewReorgAudit("code-806", "name-499");
        item.Status = ReorgAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(ReorgAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        ReorgAuditStartsInNew();
        ReorgAuditMovesToFailed();
        ReorgAuditRejectsMovesFromFailed();
    }
}
