namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AdaptiveWarrantyAuditChecks
{
    public static void AdaptiveWarrantyAuditStartsInRequested()
    {
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("code-152", "label-335");
        Check.Equal(item.Status, AdaptiveWarrantyAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveWarrantyAuditMovesToGranted()
    {
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("code-152", "label-335");
        try
        {
            item.MoveTo(AdaptiveWarrantyAuditStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveWarrantyAuditStatus.Granted, "status");
    }

    public static void AdaptiveWarrantyAuditRejectsMovesFromExpired()
    {
        var item = AdaptiveWarrantyAudit.NewAdaptiveWarrantyAudit("code-152", "label-335");
        item.Status = AdaptiveWarrantyAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptiveWarrantyAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyAuditStartsInRequested();
        AdaptiveWarrantyAuditMovesToGranted();
        AdaptiveWarrantyAuditRejectsMovesFromExpired();
    }
}
