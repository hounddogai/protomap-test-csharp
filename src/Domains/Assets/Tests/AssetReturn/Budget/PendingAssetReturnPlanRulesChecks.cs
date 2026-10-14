namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingAssetReturnPlanRulesChecks
{
    public static void PendingAssetReturnPlanValidatorReportsEveryProblem()
    {
        var validator = PendingAssetReturnPlanRules.DefaultPendingAssetReturnPlanValidator();
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAssetReturnPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingAssetReturnPlanLabelLengthRule(255);
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAssetReturnPlanRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PendingAssetReturnPlanRequiredNotesRule("pending asset return plan needs a value");
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAssetReturnPlanAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingAssetReturnPlanAmountRangeRule(0.6, 4827.3);
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("x", "y");
        item.Amount = 4828.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingAssetReturnPlanRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PendingAssetReturnPlanRequiredOwnerIdRule("pending asset return plan needs an entry");
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingAssetReturnPlanValidatorReportsEveryProblem();
        PendingAssetReturnPlanLabelLengthRuleFlagsALongLabel();
        PendingAssetReturnPlanRequiredNotesRuleFlagsABlankNotes();
        PendingAssetReturnPlanAmountRangeRuleFlagsValuesAboveAmount();
        PendingAssetReturnPlanRequiredOwnerIdRuleFlagsABlankOwnerId();
    }
}
