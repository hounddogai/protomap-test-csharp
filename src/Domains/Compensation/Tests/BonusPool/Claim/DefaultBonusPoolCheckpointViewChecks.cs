namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultBonusPoolCheckpointViewChecks
{
    public static void DefaultBonusPoolCheckpointViewShowsTheStatusInCapitals()
    {
        var item = DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint("code-611", "name-858");
        var view = DefaultBonusPoolCheckpointView.ToDefaultBonusPoolCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultBonusPoolCheckpointView.ToDefaultBonusPoolCheckpointViews(new List<DefaultBonusPoolCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultBonusPoolCheckpointViewShowsTheStatusInCapitals();
    }
}
