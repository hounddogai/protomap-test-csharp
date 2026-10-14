namespace Acme.Workforce.Assets.Tests.Warranty.Milestone;

using Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalWarrantyTargetChecks
{
    public static void SeasonalWarrantyTargetStartsInCreated()
    {
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("code-523", "title-632");
        Check.Equal(item.Status, SeasonalWarrantyTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalWarrantyTargetMovesToValidated()
    {
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("code-523", "title-632");
        try
        {
            item.MoveTo(SeasonalWarrantyTargetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalWarrantyTargetStatus.Validated, "status");
    }

    public static void SeasonalWarrantyTargetRejectsMovesFromRetired()
    {
        var item = SeasonalWarrantyTarget.NewSeasonalWarrantyTarget("code-523", "title-632");
        item.Status = SeasonalWarrantyTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalWarrantyTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalWarrantyTargetStartsInCreated();
        SeasonalWarrantyTargetMovesToValidated();
        SeasonalWarrantyTargetRejectsMovesFromRetired();
    }
}
