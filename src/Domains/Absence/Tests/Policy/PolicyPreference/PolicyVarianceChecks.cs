namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class PolicyVarianceChecks
{
    public static void PolicyVarianceStartsInCreated()
    {
        var item = PolicyVariance.NewPolicyVariance("code-307", "label-39");
        Check.Equal(item.Status, PolicyVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyVarianceMovesToPublished()
    {
        var item = PolicyVariance.NewPolicyVariance("code-307", "label-39");
        try
        {
            item.MoveTo(PolicyVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyVarianceStatus.Published, "status");
    }

    public static void PolicyVarianceRejectsMovesFromRetired()
    {
        var item = PolicyVariance.NewPolicyVariance("code-307", "label-39");
        item.Status = PolicyVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyVarianceStartsInCreated();
        PolicyVarianceMovesToPublished();
        PolicyVarianceRejectsMovesFromRetired();
    }
}
