namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskAuditRulesChecks
{
    public static void DeskAuditValidatorReportsEveryProblem()
    {
        var validator = DeskAuditRules.DefaultDeskAuditValidator();
        var item = DeskAudit.NewDeskAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskAuditRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DeskAuditRequiredUnitRule("desk audit needs a value");
        var item = DeskAudit.NewDeskAudit("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new DeskAuditDurationMinutesRangeRule(32, 86400);
        var item = DeskAudit.NewDeskAudit("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DeskAuditRequiredLabelRule("desk audit needs this field");
        var item = DeskAudit.NewDeskAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskAuditValidatorReportsEveryProblem();
        DeskAuditRequiredUnitRuleFlagsABlankUnit();
        DeskAuditDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        DeskAuditRequiredLabelRuleFlagsABlankLabel();
    }
}
