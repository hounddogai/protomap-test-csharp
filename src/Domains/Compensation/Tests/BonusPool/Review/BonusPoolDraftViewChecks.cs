namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolDraftViewChecks
{
    public static void BonusPoolDraftViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolDraft.NewBonusPoolDraft("code-243", "label-818");
        var view = BonusPoolDraftView.ToBonusPoolDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolDraftView.ToBonusPoolDraftViews(new List<BonusPoolDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolDraftViewShowsTheStatusInCapitals();
    }
}
