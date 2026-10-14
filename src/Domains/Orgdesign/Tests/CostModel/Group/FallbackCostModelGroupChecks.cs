namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FallbackCostModelGroupChecks
{
    public static void FallbackCostModelGroupStartsInCreated()
    {
        var item = FallbackCostModelGroup.NewFallbackCostModelGroup("code-755", "title-767");
        Check.Equal(item.Status, FallbackCostModelGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackCostModelGroupMovesToValidated()
    {
        var item = FallbackCostModelGroup.NewFallbackCostModelGroup("code-755", "title-767");
        try
        {
            item.MoveTo(FallbackCostModelGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackCostModelGroupStatus.Validated, "status");
    }

    public static void FallbackCostModelGroupRejectsMovesFromRetired()
    {
        var item = FallbackCostModelGroup.NewFallbackCostModelGroup("code-755", "title-767");
        item.Status = FallbackCostModelGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackCostModelGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackCostModelGroupStartsInCreated();
        FallbackCostModelGroupMovesToValidated();
        FallbackCostModelGroupRejectsMovesFromRetired();
    }
}
