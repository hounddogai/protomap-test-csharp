namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredPositionTemplateViewChecks
{
    public static void TieredPositionTemplateViewShowsTheStatusInCapitals()
    {
        var item = TieredPositionTemplate.NewTieredPositionTemplate("code-381", "label-484");
        var view = TieredPositionTemplateView.ToTieredPositionTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPositionTemplateView.ToTieredPositionTemplateViews(new List<TieredPositionTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPositionTemplateViewShowsTheStatusInCapitals();
    }
}
