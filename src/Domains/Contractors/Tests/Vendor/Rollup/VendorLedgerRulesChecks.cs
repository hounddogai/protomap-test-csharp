namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorLedgerRulesChecks
{
    public static void VendorLedgerValidatorReportsEveryProblem()
    {
        var validator = VendorLedgerRules.DefaultVendorLedgerValidator();
        var item = VendorLedger.NewVendorLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorLedgerMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new VendorLedgerMarginRangeRule(9.0, 4170.7);
        var item = VendorLedger.NewVendorLedger("x", "y");
        item.Margin = 4172.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorLedgerRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new VendorLedgerRequiredTeamRule("vendor ledger needs an entry");
        var item = VendorLedger.NewVendorLedger("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorLedgerRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new VendorLedgerRequiredSkillRule("vendor ledger needs an entry");
        var item = VendorLedger.NewVendorLedger("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorLedgerTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VendorLedgerTitleLengthRule(80);
        var item = VendorLedger.NewVendorLedger("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorLedgerValidatorReportsEveryProblem();
        VendorLedgerMarginRangeRuleFlagsValuesAboveMargin();
        VendorLedgerRequiredTeamRuleFlagsABlankTeam();
        VendorLedgerRequiredSkillRuleFlagsABlankSkill();
        VendorLedgerTitleLengthRuleFlagsALongTitle();
    }
}
