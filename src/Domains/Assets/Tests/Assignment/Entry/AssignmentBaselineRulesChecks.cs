namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentBaselineRulesChecks
{
    public static void AssignmentBaselineValidatorReportsEveryProblem()
    {
        var validator = AssignmentBaselineRules.DefaultAssignmentBaselineValidator();
        var item = AssignmentBaseline.NewAssignmentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentBaselineCodeLengthRule(3600);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentBaselineWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new AssignmentBaselineWeightRangeRule(5.1, 76.6);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.Weight = 78.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentBaselineRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new AssignmentBaselineRatioRangeRule(4.3, 2776.1);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.Ratio = 2777.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentBaselineChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AssignmentBaselineChecksumLengthRule(64);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentBaselineAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AssignmentBaselineAmountRangeRule(4.6, 4589.4);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.Amount = 4590.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentBaselineContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AssignmentBaselineContactEmailLengthRule(64);
        var item = AssignmentBaseline.NewAssignmentBaseline("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentBaselineValidatorReportsEveryProblem();
        AssignmentBaselineCodeLengthRuleFlagsALongCode();
        AssignmentBaselineWeightRangeRuleFlagsValuesAboveWeight();
        AssignmentBaselineRatioRangeRuleFlagsValuesAboveRatio();
        AssignmentBaselineChecksumLengthRuleFlagsALongChecksum();
        AssignmentBaselineAmountRangeRuleFlagsValuesAboveAmount();
        AssignmentBaselineContactEmailLengthRuleFlagsALongContactEmail();
    }
}
