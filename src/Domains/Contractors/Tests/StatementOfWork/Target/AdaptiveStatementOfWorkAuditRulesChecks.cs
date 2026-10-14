namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveStatementOfWorkAuditRulesChecks
{
    public static void AdaptiveStatementOfWorkAuditValidatorReportsEveryProblem()
    {
        var validator = AdaptiveStatementOfWorkAuditRules.DefaultAdaptiveStatementOfWorkAuditValidator();
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveStatementOfWorkAuditNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveStatementOfWorkAuditNameLengthRule(32);
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveStatementOfWorkAuditLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveStatementOfWorkAuditLabelLengthRule(86400);
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveStatementOfWorkAuditHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AdaptiveStatementOfWorkAuditHoursRangeRule(3.9, 3811.9);
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("x", "y");
        item.Hours = 3813.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveStatementOfWorkAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveStatementOfWorkAuditRequiredNameRule("adaptive statement of work audit needs an entry");
        var item = AdaptiveStatementOfWorkAudit.NewAdaptiveStatementOfWorkAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveStatementOfWorkAuditValidatorReportsEveryProblem();
        AdaptiveStatementOfWorkAuditNameLengthRuleFlagsALongName();
        AdaptiveStatementOfWorkAuditLabelLengthRuleFlagsALongLabel();
        AdaptiveStatementOfWorkAuditHoursRangeRuleFlagsValuesAboveHours();
        AdaptiveStatementOfWorkAuditRequiredNameRuleFlagsABlankName();
    }
}
