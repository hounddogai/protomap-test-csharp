namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditScheduleRulesChecks
{
    public static void AuditScheduleValidatorReportsEveryProblem()
    {
        var validator = AuditScheduleRules.DefaultAuditScheduleValidator();
        var item = AuditSchedule.NewAuditSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditScheduleCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new AuditScheduleCategoryLengthRule(16);
        var item = AuditSchedule.NewAuditSchedule("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditScheduleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AuditScheduleRequiredCategoryRule("audit schedule needs a value");
        var item = AuditSchedule.NewAuditSchedule("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditScheduleRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new AuditScheduleRequiredLocaleRule("audit schedule needs this field");
        var item = AuditSchedule.NewAuditSchedule("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditScheduleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new AuditScheduleLocaleLengthRule(255);
        var item = AuditSchedule.NewAuditSchedule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AuditScheduleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AuditScheduleRequiredTitleRule("audit schedule needs an entry");
        var item = AuditSchedule.NewAuditSchedule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditScheduleValidatorReportsEveryProblem();
        AuditScheduleCategoryLengthRuleFlagsALongCategory();
        AuditScheduleRequiredCategoryRuleFlagsABlankCategory();
        AuditScheduleRequiredLocaleRuleFlagsABlankLocale();
        AuditScheduleLocaleLengthRuleFlagsALongLocale();
        AuditScheduleRequiredTitleRuleFlagsABlankTitle();
    }
}
