namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyWarrantyBaselineChecks
{
    public static void LegacyWarrantyBaselineStartsInOpen()
    {
        var item = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline("code-627", "label-164");
        Check.Equal(item.Status, LegacyWarrantyBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyWarrantyBaselineMovesToArchived()
    {
        var item = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline("code-627", "label-164");
        try
        {
            item.MoveTo(LegacyWarrantyBaselineStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyWarrantyBaselineStatus.Archived, "status");
    }

    public static void LegacyWarrantyBaselineRejectsMovesFromArchived()
    {
        var item = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline("code-627", "label-164");
        item.Status = LegacyWarrantyBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyWarrantyBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyWarrantyBaselineStartsInOpen();
        LegacyWarrantyBaselineMovesToArchived();
        LegacyWarrantyBaselineRejectsMovesFromArchived();
    }
}
