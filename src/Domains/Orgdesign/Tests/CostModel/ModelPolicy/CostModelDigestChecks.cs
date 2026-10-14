namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class CostModelDigestChecks
{
    public static void CostModelDigestStartsInOpen()
    {
        var item = CostModelDigest.NewCostModelDigest("code-532", "label-139");
        Check.Equal(item.Status, CostModelDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelDigestMovesToResolved()
    {
        var item = CostModelDigest.NewCostModelDigest("code-532", "label-139");
        try
        {
            item.MoveTo(CostModelDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelDigestStatus.Resolved, "status");
    }

    public static void CostModelDigestRejectsMovesFromArchived()
    {
        var item = CostModelDigest.NewCostModelDigest("code-532", "label-139");
        item.Status = CostModelDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(CostModelDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        CostModelDigestStartsInOpen();
        CostModelDigestMovesToResolved();
        CostModelDigestRejectsMovesFromArchived();
    }
}
