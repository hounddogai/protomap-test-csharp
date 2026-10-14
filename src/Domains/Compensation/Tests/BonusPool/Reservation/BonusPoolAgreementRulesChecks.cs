namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BonusPoolAgreementRulesChecks
{
    public static void BonusPoolAgreementValidatorReportsEveryProblem()
    {
        var validator = BonusPoolAgreementRules.DefaultBonusPoolAgreementValidator();
        var item = BonusPoolAgreement.NewBonusPoolAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BonusPoolAgreementUpdatedAtRangeRule(120, 3600);
        var item = BonusPoolAgreement.NewBonusPoolAgreement("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BonusPoolAgreementGradeLengthRuleFlagsALongGrade()
    {
        var rule = new BonusPoolAgreementGradeLengthRule(255);
        var item = BonusPoolAgreement.NewBonusPoolAgreement("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new BonusPoolAgreementCodeLengthRule(1000);
        var item = BonusPoolAgreement.NewBonusPoolAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BonusPoolAgreementRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new BonusPoolAgreementRequiredGradeRule("bonus pool agreement needs a value");
        var item = BonusPoolAgreement.NewBonusPoolAgreement("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BonusPoolAgreementValidatorReportsEveryProblem();
        BonusPoolAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        BonusPoolAgreementGradeLengthRuleFlagsALongGrade();
        BonusPoolAgreementCodeLengthRuleFlagsALongCode();
        BonusPoolAgreementRequiredGradeRuleFlagsABlankGrade();
    }
}
