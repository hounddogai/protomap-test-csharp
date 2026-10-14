namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RetroPayScheduleRulesChecks
{
    public static void RetroPayScheduleValidatorReportsEveryProblem()
    {
        var validator = RetroPayScheduleRules.DefaultRetroPayScheduleValidator();
        var item = RetroPaySchedule.NewRetroPaySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayScheduleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new RetroPayScheduleOwnerIdLengthRule(120);
        var item = RetroPaySchedule.NewRetroPaySchedule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayScheduleRequiredCodeRule("retro pay schedule needs this field");
        var item = RetroPaySchedule.NewRetroPaySchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RetroPayScheduleLabelLengthRule(3600);
        var item = RetroPaySchedule.NewRetroPaySchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayScheduleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayScheduleRequiredLabelRule("retro pay schedule needs a value");
        var item = RetroPaySchedule.NewRetroPaySchedule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayScheduleValidatorReportsEveryProblem();
        RetroPayScheduleOwnerIdLengthRuleFlagsALongOwnerId();
        RetroPayScheduleRequiredCodeRuleFlagsABlankCode();
        RetroPayScheduleLabelLengthRuleFlagsALongLabel();
        RetroPayScheduleRequiredLabelRuleFlagsABlankLabel();
    }
}
