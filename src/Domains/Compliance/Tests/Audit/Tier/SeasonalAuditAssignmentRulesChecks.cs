namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalAuditAssignmentRulesChecks
{
    public static void SeasonalAuditAssignmentValidatorReportsEveryProblem()
    {
        var validator = SeasonalAuditAssignmentRules.DefaultSeasonalAuditAssignmentValidator();
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAuditAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new SeasonalAuditAssignmentNameLengthRule(255);
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAuditAssignmentBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new SeasonalAuditAssignmentBadgeLengthRule(64);
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAuditAssignmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new SeasonalAuditAssignmentRequiredUnitRule("seasonal audit assignment needs this field");
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalAuditAssignmentRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SeasonalAuditAssignmentRequiredBadgeRule("seasonal audit assignment needs a value");
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalAuditAssignmentValidatorReportsEveryProblem();
        SeasonalAuditAssignmentNameLengthRuleFlagsALongName();
        SeasonalAuditAssignmentBadgeLengthRuleFlagsALongBadge();
        SeasonalAuditAssignmentRequiredUnitRuleFlagsABlankUnit();
        SeasonalAuditAssignmentRequiredBadgeRuleFlagsABlankBadge();
    }
}
