namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathQuotaChecks
{
    public static void RegionalCareerPathQuotaStartsInCreated()
    {
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("code-925", "label-252");
        Check.Equal(item.Status, RegionalCareerPathQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalCareerPathQuotaMovesToValidated()
    {
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("code-925", "label-252");
        try
        {
            item.MoveTo(RegionalCareerPathQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalCareerPathQuotaStatus.Validated, "status");
    }

    public static void RegionalCareerPathQuotaRejectsMovesFromRetired()
    {
        var item = RegionalCareerPathQuota.NewRegionalCareerPathQuota("code-925", "label-252");
        item.Status = RegionalCareerPathQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalCareerPathQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalCareerPathQuotaStartsInCreated();
        RegionalCareerPathQuotaMovesToValidated();
        RegionalCareerPathQuotaRejectsMovesFromRetired();
    }
}
