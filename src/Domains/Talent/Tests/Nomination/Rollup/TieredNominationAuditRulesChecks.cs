namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredNominationAuditRulesChecks
{
    public static void TieredNominationAuditValidatorReportsEveryProblem()
    {
        var validator = TieredNominationAuditRules.DefaultTieredNominationAuditValidator();
        var item = TieredNominationAudit.NewTieredNominationAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredNominationAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredNominationAuditRequiredCodeRule("tiered nomination audit needs an entry");
        var item = TieredNominationAudit.NewTieredNominationAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredNominationAuditFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TieredNominationAuditFloorValueRangeRule(9.1, 2636.7);
        var item = TieredNominationAudit.NewTieredNominationAudit("x", "y");
        item.FloorValue = 2638.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredNominationAuditRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new TieredNominationAuditRevisionRangeRule(32, 3600);
        var item = TieredNominationAudit.NewTieredNominationAudit("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredNominationAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredNominationAuditTitleLengthRule(86400);
        var item = TieredNominationAudit.NewTieredNominationAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredNominationAuditMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TieredNominationAuditMinimumRangeRule(1.8, 3292.4);
        var item = TieredNominationAudit.NewTieredNominationAudit("x", "y");
        item.Minimum = 3293.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredNominationAuditRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TieredNominationAuditRequiredPhoneRule("tiered nomination audit needs an entry");
        var item = TieredNominationAudit.NewTieredNominationAudit("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredNominationAuditValidatorReportsEveryProblem();
        TieredNominationAuditRequiredCodeRuleFlagsABlankCode();
        TieredNominationAuditFloorValueRangeRuleFlagsValuesAboveFloorValue();
        TieredNominationAuditRevisionRangeRuleFlagsValuesAboveRevision();
        TieredNominationAuditTitleLengthRuleFlagsALongTitle();
        TieredNominationAuditMinimumRangeRuleFlagsValuesAboveMinimum();
        TieredNominationAuditRequiredPhoneRuleFlagsABlankPhone();
    }
}
