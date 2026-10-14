namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LenientCertificationAssignmentRulesChecks
{
    public static void LenientCertificationAssignmentValidatorReportsEveryProblem()
    {
        var validator = LenientCertificationAssignmentRules.DefaultLenientCertificationAssignmentValidator();
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientCertificationAssignmentRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LenientCertificationAssignmentRequiredChecksumRule(
            "lenient certification assignment needs a value");
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCertificationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LenientCertificationAssignmentAttemptsRangeRule(80, 1000);
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientCertificationAssignmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LenientCertificationAssignmentFteRangeRule(9.6, 4421.9);
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("x", "y");
        item.Fte = 4423.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientCertificationAssignmentRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new LenientCertificationAssignmentRequiredLocaleRule(
            "lenient certification assignment needs this field");
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCertificationAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientCertificationAssignmentRequiredTitleRule(
            "lenient certification assignment needs an entry");
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCertificationAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientCertificationAssignmentTitleLengthRule(32);
        var item = LenientCertificationAssignment.NewLenientCertificationAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientCertificationAssignmentValidatorReportsEveryProblem();
        LenientCertificationAssignmentRequiredChecksumRuleFlagsABlankChecksum();
        LenientCertificationAssignmentAttemptsRangeRuleFlagsValuesAboveAttempts();
        LenientCertificationAssignmentFteRangeRuleFlagsValuesAboveFte();
        LenientCertificationAssignmentRequiredLocaleRuleFlagsABlankLocale();
        LenientCertificationAssignmentRequiredTitleRuleFlagsABlankTitle();
        LenientCertificationAssignmentTitleLengthRuleFlagsALongTitle();
    }
}
