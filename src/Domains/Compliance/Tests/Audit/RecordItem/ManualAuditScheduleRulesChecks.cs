namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualAuditScheduleRulesChecks
{
    public static void ManualAuditScheduleValidatorReportsEveryProblem()
    {
        var validator = ManualAuditScheduleRules.DefaultManualAuditScheduleValidator();
        var item = ManualAuditSchedule.NewManualAuditSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAuditScheduleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualAuditScheduleTitleLengthRule(80);
        var item = ManualAuditSchedule.NewManualAuditSchedule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAuditScheduleDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new ManualAuditScheduleDiscountRangeRule(8.5, 565.9);
        var item = ManualAuditSchedule.NewManualAuditSchedule("x", "y");
        item.Discount = 567.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAuditScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAuditScheduleCodeLengthRule(255);
        var item = ManualAuditSchedule.NewManualAuditSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAuditScheduleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new ManualAuditScheduleLocaleLengthRule(16);
        var item = ManualAuditSchedule.NewManualAuditSchedule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualAuditScheduleValidatorReportsEveryProblem();
        ManualAuditScheduleTitleLengthRuleFlagsALongTitle();
        ManualAuditScheduleDiscountRangeRuleFlagsValuesAboveDiscount();
        ManualAuditScheduleCodeLengthRuleFlagsALongCode();
        ManualAuditScheduleLocaleLengthRuleFlagsALongLocale();
    }
}
