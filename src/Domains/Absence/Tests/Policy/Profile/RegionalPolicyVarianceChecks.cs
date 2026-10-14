namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalPolicyVarianceChecks
{
    public static void RegionalPolicyVarianceStartsInRequested()
    {
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("code-735", "name-224");
        Check.Equal(item.Status, RegionalPolicyVarianceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPolicyVarianceMovesToRevoked()
    {
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("code-735", "name-224");
        try
        {
            item.MoveTo(RegionalPolicyVarianceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPolicyVarianceStatus.Revoked, "status");
    }

    public static void RegionalPolicyVarianceRejectsMovesFromExpired()
    {
        var item = RegionalPolicyVariance.NewRegionalPolicyVariance("code-735", "name-224");
        item.Status = RegionalPolicyVarianceStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalPolicyVarianceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalPolicyVarianceStartsInRequested();
        RegionalPolicyVarianceMovesToRevoked();
        RegionalPolicyVarianceRejectsMovesFromExpired();
    }
}
