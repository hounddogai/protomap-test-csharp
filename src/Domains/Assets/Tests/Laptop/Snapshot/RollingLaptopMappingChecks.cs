namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingLaptopMappingChecks
{
    public static void RollingLaptopMappingStartsInRequested()
    {
        var item = RollingLaptopMapping.NewRollingLaptopMapping("code-425", "name-878");
        Check.Equal(item.Status, RollingLaptopMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLaptopMappingMovesToRevoked()
    {
        var item = RollingLaptopMapping.NewRollingLaptopMapping("code-425", "name-878");
        try
        {
            item.MoveTo(RollingLaptopMappingStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLaptopMappingStatus.Revoked, "status");
    }

    public static void RollingLaptopMappingRejectsMovesFromExpired()
    {
        var item = RollingLaptopMapping.NewRollingLaptopMapping("code-425", "name-878");
        item.Status = RollingLaptopMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingLaptopMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingLaptopMappingStartsInRequested();
        RollingLaptopMappingMovesToRevoked();
        RollingLaptopMappingRejectsMovesFromExpired();
    }
}
