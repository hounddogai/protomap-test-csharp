namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantMappingChecks
{
    public static void EquityGrantMappingStartsInCreated()
    {
        var item = EquityGrantMapping.NewEquityGrantMapping("code-746", "name-270");
        Check.Equal(item.Status, EquityGrantMappingStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantMappingMovesToPublished()
    {
        var item = EquityGrantMapping.NewEquityGrantMapping("code-746", "name-270");
        try
        {
            item.MoveTo(EquityGrantMappingStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantMappingStatus.Published, "status");
    }

    public static void EquityGrantMappingRejectsMovesFromRetired()
    {
        var item = EquityGrantMapping.NewEquityGrantMapping("code-746", "name-270");
        item.Status = EquityGrantMappingStatus.Retired;
        Check.Fails(() => item.MoveTo(EquityGrantMappingStatus.Created), "move");
    }

    public static void RunAll()
    {
        EquityGrantMappingStartsInCreated();
        EquityGrantMappingMovesToPublished();
        EquityGrantMappingRejectsMovesFromRetired();
    }
}
