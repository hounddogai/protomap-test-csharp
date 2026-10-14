namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyRollupChecks
{
    public static void WarrantyRollupStartsInCreated()
    {
        var item = WarrantyRollup.NewWarrantyRollup("code-798", "name-446");
        Check.Equal(item.Status, WarrantyRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyRollupMovesToRetired()
    {
        var item = WarrantyRollup.NewWarrantyRollup("code-798", "name-446");
        try
        {
            item.MoveTo(WarrantyRollupStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyRollupStatus.Retired, "status");
    }

    public static void WarrantyRollupRejectsMovesFromRetired()
    {
        var item = WarrantyRollup.NewWarrantyRollup("code-798", "name-446");
        item.Status = WarrantyRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(WarrantyRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        WarrantyRollupStartsInCreated();
        WarrantyRollupMovesToRetired();
        WarrantyRollupRejectsMovesFromRetired();
    }
}
