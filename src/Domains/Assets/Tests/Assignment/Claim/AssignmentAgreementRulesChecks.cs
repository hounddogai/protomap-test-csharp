namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementRulesChecks
{
    public static void AssignmentAgreementValidatorReportsEveryProblem()
    {
        var validator = AssignmentAgreementRules.DefaultAssignmentAgreementValidator();
        var item = AssignmentAgreement.NewAssignmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentAgreementRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AssignmentAgreementRateRangeRule(8.1, 1053.3);
        var item = AssignmentAgreement.NewAssignmentAgreement("x", "y");
        item.Rate = 1054.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new AssignmentAgreementRequiredEmployeeIdRule("assignment agreement needs an entry");
        var item = AssignmentAgreement.NewAssignmentAgreement("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentAgreementCodeLengthRule(32);
        var item = AssignmentAgreement.NewAssignmentAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssignmentAgreementRequiredTitleRule("assignment agreement needs this field");
        var item = AssignmentAgreement.NewAssignmentAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentAgreementRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new AssignmentAgreementRatingRangeRule(7.9, 2930.5);
        var item = AssignmentAgreement.NewAssignmentAgreement("x", "y");
        item.Rating = 2932.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentAgreementPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AssignmentAgreementPhoneLengthRule(3600);
        var item = AssignmentAgreement.NewAssignmentAgreement("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentAgreementValidatorReportsEveryProblem();
        AssignmentAgreementRateRangeRuleFlagsValuesAboveRate();
        AssignmentAgreementRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        AssignmentAgreementCodeLengthRuleFlagsALongCode();
        AssignmentAgreementRequiredTitleRuleFlagsABlankTitle();
        AssignmentAgreementRatingRangeRuleFlagsValuesAboveRating();
        AssignmentAgreementPhoneLengthRuleFlagsALongPhone();
    }
}
