namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CappedCostModelDraftViewChecks
{
    public static void CappedCostModelDraftViewShowsTheStatusInCapitals()
    {
        var item = CappedCostModelDraft.NewCappedCostModelDraft("code-321", "title-228");
        var view = CappedCostModelDraftView.ToCappedCostModelDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedCostModelDraftView.ToCappedCostModelDraftViews(new List<CappedCostModelDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedCostModelDraftViewShowsTheStatusInCapitals();
    }
}
