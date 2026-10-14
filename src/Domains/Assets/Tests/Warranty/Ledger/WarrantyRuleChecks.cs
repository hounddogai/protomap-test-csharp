namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WarrantyRuleChecks
{
    public static void WarrantyRuleStartsInProposed()
    {
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
        Check.Equal(item.Status, WarrantyRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyRuleMovesToWithdrawn()
    {
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
        try
        {
            item.MoveTo(WarrantyRuleStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyRuleStatus.Withdrawn, "status");
    }

    public static void WarrantyRuleRejectsMovesFromWithdrawn()
    {
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
        item.Status = WarrantyRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WarrantyRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WarrantyRuleStartsInProposed();
        WarrantyRuleMovesToWithdrawn();
        WarrantyRuleRejectsMovesFromWithdrawn();
    }
}
