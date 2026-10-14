namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineChecks
{
    public static void CappedWarrantyBaselineStartsInCreated()
    {
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
        Check.Equal(item.Status, CappedWarrantyBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedWarrantyBaselineMovesToPublished()
    {
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
        try
        {
            item.MoveTo(CappedWarrantyBaselineStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedWarrantyBaselineStatus.Published, "status");
    }

    public static void CappedWarrantyBaselineRejectsMovesFromRetired()
    {
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
        item.Status = CappedWarrantyBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedWarrantyBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedWarrantyBaselineStartsInCreated();
        CappedWarrantyBaselineMovesToPublished();
        CappedWarrantyBaselineRejectsMovesFromRetired();
    }
}
