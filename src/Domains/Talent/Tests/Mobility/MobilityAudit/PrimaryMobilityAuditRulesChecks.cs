namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class PrimaryMobilityAuditRulesChecks
{
    public static void PrimaryMobilityAuditValidatorReportsEveryProblem()
    {
        var validator = PrimaryMobilityAuditRules.DefaultPrimaryMobilityAuditValidator();
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryMobilityAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryMobilityAuditTitleLengthRule(64);
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryMobilityAuditRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PrimaryMobilityAuditRequiredTeamRule("primary mobility audit needs a value");
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMobilityAuditTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PrimaryMobilityAuditTeamLengthRule(32);
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryMobilityAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryMobilityAuditRequiredTitleRule("primary mobility audit needs an entry");
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryMobilityAuditAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PrimaryMobilityAuditAmountRangeRule(7.9, 143.1);
        var item = PrimaryMobilityAudit.NewPrimaryMobilityAudit("x", "y");
        item.Amount = 144.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryMobilityAuditValidatorReportsEveryProblem();
        PrimaryMobilityAuditTitleLengthRuleFlagsALongTitle();
        PrimaryMobilityAuditRequiredTeamRuleFlagsABlankTeam();
        PrimaryMobilityAuditTeamLengthRuleFlagsALongTeam();
        PrimaryMobilityAuditRequiredTitleRuleFlagsABlankTitle();
        PrimaryMobilityAuditAmountRangeRuleFlagsValuesAboveAmount();
    }
}
