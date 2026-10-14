namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LegacyCostModelGroupChecks
{
    public static void LegacyCostModelGroupStartsInCreated()
    {
        var item = LegacyCostModelGroup.NewLegacyCostModelGroup("code-168", "label-705");
        Check.Equal(item.Status, LegacyCostModelGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyCostModelGroupMovesToPublished()
    {
        var item = LegacyCostModelGroup.NewLegacyCostModelGroup("code-168", "label-705");
        try
        {
            item.MoveTo(LegacyCostModelGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyCostModelGroupStatus.Published, "status");
    }

    public static void LegacyCostModelGroupRejectsMovesFromRetired()
    {
        var item = LegacyCostModelGroup.NewLegacyCostModelGroup("code-168", "label-705");
        item.Status = LegacyCostModelGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyCostModelGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyCostModelGroupStartsInCreated();
        LegacyCostModelGroupMovesToPublished();
        LegacyCostModelGroupRejectsMovesFromRetired();
    }
}
