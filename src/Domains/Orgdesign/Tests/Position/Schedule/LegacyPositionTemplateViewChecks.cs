namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyPositionTemplateViewChecks
{
    public static void LegacyPositionTemplateViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("code-371", "name-815");
        var view = LegacyPositionTemplateView.ToLegacyPositionTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionTemplateView.ToLegacyPositionTemplateViews(new List<LegacyPositionTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionTemplateViewShowsTheStatusInCapitals();
    }
}
