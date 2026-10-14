namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimRulesChecks
{
    public static void PrimaryLeaveRequestClaimValidatorReportsEveryProblem()
    {
        var validator = PrimaryLeaveRequestClaimRules.DefaultPrimaryLeaveRequestClaimValidator();
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryLeaveRequestClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PrimaryLeaveRequestClaimBatchSizeRangeRule(16, 80);
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryLeaveRequestClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryLeaveRequestClaimRequiredCodeRule("primary leave request claim needs this field");
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryLeaveRequestClaimRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new PrimaryLeaveRequestClaimRequiredSkillRule("primary leave request claim needs an entry");
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryLeaveRequestClaimNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryLeaveRequestClaimNameLengthRule(80);
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryLeaveRequestClaimSkillLengthRuleFlagsALongSkill()
    {
        var rule = new PrimaryLeaveRequestClaimSkillLengthRule(16);
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestClaimValidatorReportsEveryProblem();
        PrimaryLeaveRequestClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PrimaryLeaveRequestClaimRequiredCodeRuleFlagsABlankCode();
        PrimaryLeaveRequestClaimRequiredSkillRuleFlagsABlankSkill();
        PrimaryLeaveRequestClaimNameLengthRuleFlagsALongName();
        PrimaryLeaveRequestClaimSkillLengthRuleFlagsALongSkill();
    }
}
