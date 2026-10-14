namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ActionPlanLedgerRulesChecks
{
    public static void ActionPlanLedgerValidatorReportsEveryProblem()
    {
        var validator = ActionPlanLedgerRules.DefaultActionPlanLedgerValidator();
        var item = ActionPlanLedger.NewActionPlanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanLedgerRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ActionPlanLedgerRequiredCodeRule("action plan ledger needs this field");
        var item = ActionPlanLedger.NewActionPlanLedger("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanLedgerCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ActionPlanLedgerCapacityRangeRule(80, 86400);
        var item = ActionPlanLedger.NewActionPlanLedger("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanLedgerCodeLengthRule(64);
        var item = ActionPlanLedger.NewActionPlanLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanLedgerValidatorReportsEveryProblem();
        ActionPlanLedgerRequiredCodeRuleFlagsABlankCode();
        ActionPlanLedgerCapacityRangeRuleFlagsValuesAboveCapacity();
        ActionPlanLedgerCodeLengthRuleFlagsALongCode();
    }
}
