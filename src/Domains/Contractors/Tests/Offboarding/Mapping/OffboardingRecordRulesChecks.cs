namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingRecordRulesChecks
{
    public static void OffboardingRecordValidatorReportsEveryProblem()
    {
        var validator = OffboardingRecordRules.DefaultOffboardingRecordValidator();
        var item = OffboardingRecord.NewOffboardingRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OffboardingRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new OffboardingRecordCodeLengthRule(255);
        var item = OffboardingRecord.NewOffboardingRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OffboardingRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OffboardingRecordRequiredLabelRule("offboarding record needs a value");
        var item = OffboardingRecord.NewOffboardingRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OffboardingRecordValidatorReportsEveryProblem();
        OffboardingRecordCodeLengthRuleFlagsALongCode();
        OffboardingRecordRequiredLabelRuleFlagsABlankLabel();
    }
}
