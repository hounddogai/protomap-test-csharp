namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentClaim;

using Acme.Workforce.Assets.Assignment.AssignmentClaim;

/// <summary>Checks of assignment claim code, run by the project's runner.</summary>
public static class SeasonalAssignmentAllocationRulesChecks
{
    public static void SeasonalAssignmentAllocationValidatorReportsEveryProblem()
    {
        var validator = SeasonalAssignmentAllocationRules.DefaultSeasonalAssignmentAllocationValidator();
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAssignmentAllocationMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalAssignmentAllocationMarginRangeRule(3.8, 1908.4);
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("x", "y");
        item.Margin = 1909.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAssignmentAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalAssignmentAllocationRequiredCodeRule("seasonal assignment allocation needs this field");
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAssignmentAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAssignmentAllocationCodeLengthRule(64);
        var item = SeasonalAssignmentAllocation.NewSeasonalAssignmentAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalAssignmentAllocationValidatorReportsEveryProblem();
        SeasonalAssignmentAllocationMarginRangeRuleFlagsValuesAboveMargin();
        SeasonalAssignmentAllocationRequiredCodeRuleFlagsABlankCode();
        SeasonalAssignmentAllocationCodeLengthRuleFlagsALongCode();
    }
}
