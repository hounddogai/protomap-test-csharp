namespace Acme.Workforce.Orgdesign.Tests.Headcount.Quota;

using Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DefaultHeadcountQuotaChecks
{
    public static void DefaultHeadcountQuotaStartsInDraft()
    {
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
        Check.Equal(item.Status, DefaultHeadcountQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountQuotaMovesToPending()
    {
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
        try
        {
            item.MoveTo(DefaultHeadcountQuotaStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountQuotaStatus.Pending, "status");
    }

    public static void DefaultHeadcountQuotaRejectsMovesFromClosed()
    {
        var item = DefaultHeadcountQuota.NewDefaultHeadcountQuota("code-241", "name-616");
        item.Status = DefaultHeadcountQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultHeadcountQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountQuotaStartsInDraft();
        DefaultHeadcountQuotaMovesToPending();
        DefaultHeadcountQuotaRejectsMovesFromClosed();
    }
}
