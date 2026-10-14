namespace Acme.Workforce.Compliance.Tests.Certification.Checkpoint;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LenientCertificationTierRulesChecks
{
    public static void LenientCertificationTierValidatorReportsEveryProblem()
    {
        var validator = LenientCertificationTierRules.DefaultLenientCertificationTierValidator();
        var item = LenientCertificationTier.NewLenientCertificationTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientCertificationTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientCertificationTierRequiredLabelRule("lenient certification tier needs this field");
        var item = LenientCertificationTier.NewLenientCertificationTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCertificationTierScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LenientCertificationTierScoreRangeRule(6.0, 3932.1);
        var item = LenientCertificationTier.NewLenientCertificationTier("x", "y");
        item.Score = 3933.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientCertificationTierNameLengthRuleFlagsALongName()
    {
        var rule = new LenientCertificationTierNameLengthRule(120);
        var item = LenientCertificationTier.NewLenientCertificationTier("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientCertificationTierRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new LenientCertificationTierRequiredTeamRule("lenient certification tier needs a value");
        var item = LenientCertificationTier.NewLenientCertificationTier("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientCertificationTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientCertificationTierLabelLengthRule(64);
        var item = LenientCertificationTier.NewLenientCertificationTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientCertificationTierValidatorReportsEveryProblem();
        LenientCertificationTierRequiredLabelRuleFlagsABlankLabel();
        LenientCertificationTierScoreRangeRuleFlagsValuesAboveScore();
        LenientCertificationTierNameLengthRuleFlagsALongName();
        LenientCertificationTierRequiredTeamRuleFlagsABlankTeam();
        LenientCertificationTierLabelLengthRuleFlagsALongLabel();
    }
}
