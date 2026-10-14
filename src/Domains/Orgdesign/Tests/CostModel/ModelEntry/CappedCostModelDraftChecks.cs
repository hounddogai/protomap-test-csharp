namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CappedCostModelDraftChecks
{
    public static void CappedCostModelDraftStartsInRequested()
    {
        var item = CappedCostModelDraft.NewCappedCostModelDraft("code-321", "title-228");
        Check.Equal(item.Status, CappedCostModelDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCostModelDraftMovesToRevoked()
    {
        var item = CappedCostModelDraft.NewCappedCostModelDraft("code-321", "title-228");
        try
        {
            item.MoveTo(CappedCostModelDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCostModelDraftStatus.Revoked, "status");
    }

    public static void CappedCostModelDraftRejectsMovesFromExpired()
    {
        var item = CappedCostModelDraft.NewCappedCostModelDraft("code-321", "title-228");
        item.Status = CappedCostModelDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedCostModelDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedCostModelDraftStartsInRequested();
        CappedCostModelDraftMovesToRevoked();
        CappedCostModelDraftRejectsMovesFromExpired();
    }
}
