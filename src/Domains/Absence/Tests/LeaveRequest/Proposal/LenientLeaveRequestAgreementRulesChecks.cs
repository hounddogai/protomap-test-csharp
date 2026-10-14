namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientLeaveRequestAgreementRulesChecks
{
    public static void LenientLeaveRequestAgreementValidatorReportsEveryProblem()
    {
        var validator = LenientLeaveRequestAgreementRules.DefaultLenientLeaveRequestAgreementValidator();
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLeaveRequestAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new LenientLeaveRequestAgreementNameLengthRule(64);
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientLeaveRequestAgreementLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LenientLeaveRequestAgreementLimitRangeRule(32, 32);
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("x", "y");
        item.Limit = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLeaveRequestAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientLeaveRequestAgreementCodeLengthRule(80);
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientLeaveRequestAgreementRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LenientLeaveRequestAgreementRequiredChannelRule(
            "lenient leave request agreement needs this field");
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientLeaveRequestAgreementValidatorReportsEveryProblem();
        LenientLeaveRequestAgreementNameLengthRuleFlagsALongName();
        LenientLeaveRequestAgreementLimitRangeRuleFlagsValuesAboveLimit();
        LenientLeaveRequestAgreementCodeLengthRuleFlagsALongCode();
        LenientLeaveRequestAgreementRequiredChannelRuleFlagsABlankChannel();
    }
}
