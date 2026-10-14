namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyPolicyChecks
{
    public static void WarrantyPolicyStartsInCreated()
    {
        var item = WarrantyPolicy.NewWarrantyPolicy("code-199", "label-910");
        Check.Equal(item.Status, WarrantyPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyPolicyMovesToPublished()
    {
        var item = WarrantyPolicy.NewWarrantyPolicy("code-199", "label-910");
        try
        {
            item.MoveTo(WarrantyPolicyStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyPolicyStatus.Published, "status");
    }

    public static void WarrantyPolicyRejectsMovesFromRetired()
    {
        var item = WarrantyPolicy.NewWarrantyPolicy("code-199", "label-910");
        item.Status = WarrantyPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(WarrantyPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        WarrantyPolicyStartsInCreated();
        WarrantyPolicyMovesToPublished();
        WarrantyPolicyRejectsMovesFromRetired();
    }
}
