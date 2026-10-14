namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PositionDraftViewChecks
{
    public static void PositionDraftViewShowsTheStatusInCapitals()
    {
        var item = PositionDraft.NewPositionDraft("code-864", "name-359");
        var view = PositionDraftView.ToPositionDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionDraftView.ToPositionDraftViews(new List<PositionDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionDraftViewShowsTheStatusInCapitals();
    }
}
