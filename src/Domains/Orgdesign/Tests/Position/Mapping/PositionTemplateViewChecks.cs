namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateViewChecks
{
    public static void PositionTemplateViewShowsTheStatusInCapitals()
    {
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
        var view = PositionTemplateView.ToPositionTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionTemplateView.ToPositionTemplateViews(new List<PositionTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionTemplateViewShowsTheStatusInCapitals();
    }
}
