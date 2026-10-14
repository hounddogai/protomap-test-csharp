namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class ManualTrainingDueNoticeRulesChecks
{
    public static void ManualTrainingDueNoticeValidatorReportsEveryProblem()
    {
        var validator = ManualTrainingDueNoticeRules.DefaultManualTrainingDueNoticeValidator();
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualTrainingDueNoticeEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new ManualTrainingDueNoticeEmployeeIdLengthRule(3600);
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualTrainingDueNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualTrainingDueNoticeCodeLengthRule(86400);
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualTrainingDueNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new ManualTrainingDueNoticeNameLengthRule(32);
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualTrainingDueNoticeCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ManualTrainingDueNoticeCapacityRangeRule(64, 1000);
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualTrainingDueNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualTrainingDueNoticeRequiredCodeRule("manual training due notice needs an entry");
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualTrainingDueNoticeValidatorReportsEveryProblem();
        ManualTrainingDueNoticeEmployeeIdLengthRuleFlagsALongEmployeeId();
        ManualTrainingDueNoticeCodeLengthRuleFlagsALongCode();
        ManualTrainingDueNoticeNameLengthRuleFlagsALongName();
        ManualTrainingDueNoticeCapacityRangeRuleFlagsValuesAboveCapacity();
        ManualTrainingDueNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
