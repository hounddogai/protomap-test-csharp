namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class CappedTaxFormRuleChecks
{
    public static void CappedTaxFormRuleStartsInProposed()
    {
        var item = CappedTaxFormRule.NewCappedTaxFormRule("code-573", "name-111");
        Check.Equal(item.Status, CappedTaxFormRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTaxFormRuleMovesToApproved()
    {
        var item = CappedTaxFormRule.NewCappedTaxFormRule("code-573", "name-111");
        try
        {
            item.MoveTo(CappedTaxFormRuleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTaxFormRuleStatus.Approved, "status");
    }

    public static void CappedTaxFormRuleRejectsMovesFromWithdrawn()
    {
        var item = CappedTaxFormRule.NewCappedTaxFormRule("code-573", "name-111");
        item.Status = CappedTaxFormRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedTaxFormRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedTaxFormRuleStartsInProposed();
        CappedTaxFormRuleMovesToApproved();
        CappedTaxFormRuleRejectsMovesFromWithdrawn();
    }
}
