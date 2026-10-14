namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class OffboardingOverrideChecks
{
    public static void OffboardingOverrideStartsInCreated()
    {
        var item = OffboardingOverride.NewOffboardingOverride("code-347", "label-859");
        Check.Equal(item.Status, OffboardingOverrideStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingOverrideMovesToValidated()
    {
        var item = OffboardingOverride.NewOffboardingOverride("code-347", "label-859");
        try
        {
            item.MoveTo(OffboardingOverrideStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingOverrideStatus.Validated, "status");
    }

    public static void OffboardingOverrideRejectsMovesFromRetired()
    {
        var item = OffboardingOverride.NewOffboardingOverride("code-347", "label-859");
        item.Status = OffboardingOverrideStatus.Retired;
        Check.Fails(() => item.MoveTo(OffboardingOverrideStatus.Created), "move");
    }

    public static void RunAll()
    {
        OffboardingOverrideStartsInCreated();
        OffboardingOverrideMovesToValidated();
        OffboardingOverrideRejectsMovesFromRetired();
    }
}
