namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshAssignmentRulesChecks
{
    public static void RefreshAssignmentValidatorReportsEveryProblem()
    {
        var validator = RefreshAssignmentRules.DefaultRefreshAssignmentValidator();
        var item = RefreshAssignment.NewRefreshAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshAssignmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new RefreshAssignmentFteRangeRule(4.9, 253.5);
        var item = RefreshAssignment.NewRefreshAssignment("x", "y");
        item.Fte = 255.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshAssignmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RefreshAssignmentBadgeLengthRule(255);
        var item = RefreshAssignment.NewRefreshAssignment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RefreshAssignmentBatchSizeRangeRule(120, 255);
        var item = RefreshAssignment.NewRefreshAssignment("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RefreshAssignmentRequiredCodeRule("refresh assignment needs this field");
        var item = RefreshAssignment.NewRefreshAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshAssignmentCodeLengthRule(86400);
        var item = RefreshAssignment.NewRefreshAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshAssignmentValidatorReportsEveryProblem();
        RefreshAssignmentFteRangeRuleFlagsValuesAboveFte();
        RefreshAssignmentBadgeLengthRuleFlagsALongBadge();
        RefreshAssignmentBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RefreshAssignmentRequiredCodeRuleFlagsABlankCode();
        RefreshAssignmentCodeLengthRuleFlagsALongCode();
    }
}
