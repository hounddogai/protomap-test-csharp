namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountQuotaChecks
{
    public static void HeadcountQuotaStartsInCreated()
    {
        var item = HeadcountQuota.NewHeadcountQuota("code-655", "label-859");
        Check.Equal(item.Status, HeadcountQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountQuotaMovesToValidated()
    {
        var item = HeadcountQuota.NewHeadcountQuota("code-655", "label-859");
        try
        {
            item.MoveTo(HeadcountQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountQuotaStatus.Validated, "status");
    }

    public static void HeadcountQuotaRejectsMovesFromRetired()
    {
        var item = HeadcountQuota.NewHeadcountQuota("code-655", "label-859");
        item.Status = HeadcountQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(HeadcountQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        HeadcountQuotaStartsInCreated();
        HeadcountQuotaMovesToValidated();
        HeadcountQuotaRejectsMovesFromRetired();
    }
}
