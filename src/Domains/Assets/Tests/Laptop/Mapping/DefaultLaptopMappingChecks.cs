namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopMappingChecks
{
    public static void DefaultLaptopMappingStartsInProposed()
    {
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("code-378", "name-257");
        Check.Equal(item.Status, DefaultLaptopMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLaptopMappingMovesToRejected()
    {
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("code-378", "name-257");
        try
        {
            item.MoveTo(DefaultLaptopMappingStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLaptopMappingStatus.Rejected, "status");
    }

    public static void DefaultLaptopMappingRejectsMovesFromWithdrawn()
    {
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("code-378", "name-257");
        item.Status = DefaultLaptopMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultLaptopMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultLaptopMappingStartsInProposed();
        DefaultLaptopMappingMovesToRejected();
        DefaultLaptopMappingRejectsMovesFromWithdrawn();
    }
}
