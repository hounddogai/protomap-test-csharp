namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class LegacyHeadcountQuotaChecks
{
    public static void LegacyHeadcountQuotaStartsInOpen()
    {
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("code-743", "label-929");
        Check.Equal(item.Status, LegacyHeadcountQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyHeadcountQuotaMovesToInReview()
    {
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("code-743", "label-929");
        try
        {
            item.MoveTo(LegacyHeadcountQuotaStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyHeadcountQuotaStatus.InReview, "status");
    }

    public static void LegacyHeadcountQuotaRejectsMovesFromArchived()
    {
        var item = LegacyHeadcountQuota.NewLegacyHeadcountQuota("code-743", "label-929");
        item.Status = LegacyHeadcountQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyHeadcountQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyHeadcountQuotaStartsInOpen();
        LegacyHeadcountQuotaMovesToInReview();
        LegacyHeadcountQuotaRejectsMovesFromArchived();
    }
}
