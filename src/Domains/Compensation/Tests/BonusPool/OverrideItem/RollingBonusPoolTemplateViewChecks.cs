namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingBonusPoolTemplateViewChecks
{
    public static void RollingBonusPoolTemplateViewShowsTheStatusInCapitals()
    {
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("code-995", "label-141");
        var view = RollingBonusPoolTemplateView.ToRollingBonusPoolTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBonusPoolTemplateView.ToRollingBonusPoolTemplateViews(new List<RollingBonusPoolTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBonusPoolTemplateViewShowsTheStatusInCapitals();
    }
}
