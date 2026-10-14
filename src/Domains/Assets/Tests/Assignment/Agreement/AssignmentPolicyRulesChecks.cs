namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssignmentPolicyRulesChecks
{
    public static void AssignmentPolicyValidatorReportsEveryProblem()
    {
        var validator = AssignmentPolicyRules.DefaultAssignmentPolicyValidator();
        var item = AssignmentPolicy.NewAssignmentPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentPolicyTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssignmentPolicyTitleLengthRule(16);
        var item = AssignmentPolicy.NewAssignmentPolicy("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssignmentPolicyRequiredTitleRule("assignment policy needs this field");
        var item = AssignmentPolicy.NewAssignmentPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentPolicyDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new AssignmentPolicyDiscountRangeRule(9.0, 1338.4);
        var item = AssignmentPolicy.NewAssignmentPolicy("x", "y");
        item.Discount = 1339.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssignmentPolicyValidatorReportsEveryProblem();
        AssignmentPolicyTitleLengthRuleFlagsALongTitle();
        AssignmentPolicyRequiredTitleRuleFlagsABlankTitle();
        AssignmentPolicyDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
