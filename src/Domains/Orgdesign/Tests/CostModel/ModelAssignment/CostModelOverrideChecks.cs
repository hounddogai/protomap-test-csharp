namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelOverrideChecks
{
    public static void CostModelOverrideStartsInRequested()
    {
        var item = CostModelOverride.NewCostModelOverride("code-748", "title-933");
        Check.Equal(item.Status, CostModelOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelOverrideMovesToRevoked()
    {
        var item = CostModelOverride.NewCostModelOverride("code-748", "title-933");
        try
        {
            item.MoveTo(CostModelOverrideStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelOverrideStatus.Revoked, "status");
    }

    public static void CostModelOverrideRejectsMovesFromExpired()
    {
        var item = CostModelOverride.NewCostModelOverride("code-748", "title-933");
        item.Status = CostModelOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelOverrideStartsInRequested();
        CostModelOverrideMovesToRevoked();
        CostModelOverrideRejectsMovesFromExpired();
    }
}
