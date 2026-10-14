namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class PrimaryRetroPayAuditChecks
{
    public static void PrimaryRetroPayAuditStartsInCreated()
    {
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("code-326", "label-744");
        Check.Equal(item.Status, PrimaryRetroPayAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRetroPayAuditMovesToPublished()
    {
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("code-326", "label-744");
        try
        {
            item.MoveTo(PrimaryRetroPayAuditStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRetroPayAuditStatus.Published, "status");
    }

    public static void PrimaryRetroPayAuditRejectsMovesFromRetired()
    {
        var item = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit("code-326", "label-744");
        item.Status = PrimaryRetroPayAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryRetroPayAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryRetroPayAuditStartsInCreated();
        PrimaryRetroPayAuditMovesToPublished();
        PrimaryRetroPayAuditRejectsMovesFromRetired();
    }
}
