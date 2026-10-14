namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopSegmentChecks
{
    public static void LaptopSegmentStartsInRequested()
    {
        var item = LaptopSegment.NewLaptopSegment("code-342", "name-13");
        Check.Equal(item.Status, LaptopSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopSegmentMovesToExpired()
    {
        var item = LaptopSegment.NewLaptopSegment("code-342", "name-13");
        try
        {
            item.MoveTo(LaptopSegmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopSegmentStatus.Expired, "status");
    }

    public static void LaptopSegmentRejectsMovesFromExpired()
    {
        var item = LaptopSegment.NewLaptopSegment("code-342", "name-13");
        item.Status = LaptopSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopSegmentStartsInRequested();
        LaptopSegmentMovesToExpired();
        LaptopSegmentRejectsMovesFromExpired();
    }
}
