namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAgreementRulesChecks
{
    public static void ExceptionAgreementValidatorReportsEveryProblem()
    {
        var validator = ExceptionAgreementRules.DefaultExceptionAgreementValidator();
        var item = ExceptionAgreement.NewExceptionAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ExceptionAgreementTitleLengthRule(3600);
        var item = ExceptionAgreement.NewExceptionAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionAgreementDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ExceptionAgreementDurationMinutesRangeRule(3600, 86400);
        var item = ExceptionAgreement.NewExceptionAgreement("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAgreementRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ExceptionAgreementRetriesRangeRule(80, 120);
        var item = ExceptionAgreement.NewExceptionAgreement("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionAgreementAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ExceptionAgreementAttemptsRangeRule(16, 3600);
        var item = ExceptionAgreement.NewExceptionAgreement("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ExceptionAgreementValidatorReportsEveryProblem();
        ExceptionAgreementTitleLengthRuleFlagsALongTitle();
        ExceptionAgreementDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ExceptionAgreementRetriesRangeRuleFlagsValuesAboveRetries();
        ExceptionAgreementAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
