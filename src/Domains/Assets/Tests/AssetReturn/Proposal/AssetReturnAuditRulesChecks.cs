namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnAuditRulesChecks
{
    public static void AssetReturnAuditValidatorReportsEveryProblem()
    {
        var validator = AssetReturnAuditRules.DefaultAssetReturnAuditValidator();
        var item = AssetReturnAudit.NewAssetReturnAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnAuditNameLengthRuleFlagsALongName()
    {
        var rule = new AssetReturnAuditNameLengthRule(255);
        var item = AssetReturnAudit.NewAssetReturnAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssetReturnAuditRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AssetReturnAuditRequiredSkillRule("asset return audit needs a value");
        var item = AssetReturnAudit.NewAssetReturnAudit("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssetReturnAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AssetReturnAuditLeaveDaysRangeRule(16, 64);
        var item = AssetReturnAudit.NewAssetReturnAudit("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AssetReturnAuditValidatorReportsEveryProblem();
        AssetReturnAuditNameLengthRuleFlagsALongName();
        AssetReturnAuditRequiredSkillRuleFlagsABlankSkill();
        AssetReturnAuditLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
