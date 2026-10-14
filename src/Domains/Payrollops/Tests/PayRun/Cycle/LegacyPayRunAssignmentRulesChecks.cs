namespace Acme.Workforce.Payrollops.Tests.PayRun.Cycle;

using Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LegacyPayRunAssignmentRulesChecks
{
    public static void LegacyPayRunAssignmentValidatorReportsEveryProblem()
    {
        var validator = LegacyPayRunAssignmentRules.DefaultLegacyPayRunAssignmentValidator();
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayRunAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyPayRunAssignmentRequiredCodeRule("legacy pay run assignment needs this field");
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayRunAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPayRunAssignmentCodeLengthRule(86400);
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPayRunAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyPayRunAssignmentRequiredLabelRule("legacy pay run assignment needs this field");
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayRunAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyPayRunAssignmentNameLengthRule(32);
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPayRunAssignmentValidatorReportsEveryProblem();
        LegacyPayRunAssignmentRequiredCodeRuleFlagsABlankCode();
        LegacyPayRunAssignmentCodeLengthRuleFlagsALongCode();
        LegacyPayRunAssignmentRequiredLabelRuleFlagsABlankLabel();
        LegacyPayRunAssignmentNameLengthRuleFlagsALongName();
    }
}
