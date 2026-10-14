namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolTemplateViewChecks
{
    public static void BonusPoolTemplateViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
        var view = BonusPoolTemplateView.ToBonusPoolTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolTemplateView.ToBonusPoolTemplateViews(new List<BonusPoolTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolTemplateViewShowsTheStatusInCapitals();
    }
}
