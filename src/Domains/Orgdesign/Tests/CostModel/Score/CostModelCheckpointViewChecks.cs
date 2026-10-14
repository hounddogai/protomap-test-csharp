namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelCheckpointViewChecks
{
    public static void CostModelCheckpointViewShowsTheStatusInCapitals()
    {
        var item = CostModelCheckpoint.NewCostModelCheckpoint("code-308", "name-322");
        var view = CostModelCheckpointView.ToCostModelCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelCheckpointView.ToCostModelCheckpointViews(new List<CostModelCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelCheckpointViewShowsTheStatusInCapitals();
    }
}
