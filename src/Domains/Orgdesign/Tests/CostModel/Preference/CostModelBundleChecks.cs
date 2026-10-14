namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelBundleChecks
{
    public static void CostModelBundleStartsInProposed()
    {
        var item = CostModelBundle.NewCostModelBundle("code-814", "label-41");
        Check.Equal(item.Status, CostModelBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelBundleMovesToRejected()
    {
        var item = CostModelBundle.NewCostModelBundle("code-814", "label-41");
        try
        {
            item.MoveTo(CostModelBundleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelBundleStatus.Rejected, "status");
    }

    public static void CostModelBundleRejectsMovesFromWithdrawn()
    {
        var item = CostModelBundle.NewCostModelBundle("code-814", "label-41");
        item.Status = CostModelBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelBundleStartsInProposed();
        CostModelBundleMovesToRejected();
        CostModelBundleRejectsMovesFromWithdrawn();
    }
}
