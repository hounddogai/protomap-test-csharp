namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultPositionQuotaChecks
{
    public static void DefaultPositionQuotaStartsInOpen()
    {
        var item = DefaultPositionQuota.NewDefaultPositionQuota("code-405", "label-114");
        Check.Equal(item.Status, DefaultPositionQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPositionQuotaMovesToResolved()
    {
        var item = DefaultPositionQuota.NewDefaultPositionQuota("code-405", "label-114");
        try
        {
            item.MoveTo(DefaultPositionQuotaStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPositionQuotaStatus.Resolved, "status");
    }

    public static void DefaultPositionQuotaRejectsMovesFromArchived()
    {
        var item = DefaultPositionQuota.NewDefaultPositionQuota("code-405", "label-114");
        item.Status = DefaultPositionQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultPositionQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultPositionQuotaStartsInOpen();
        DefaultPositionQuotaMovesToResolved();
        DefaultPositionQuotaRejectsMovesFromArchived();
    }
}
