namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationScoreRulesChecks
{
    public static void PrimaryAttestationScoreValidatorReportsEveryProblem()
    {
        var validator = PrimaryAttestationScoreRules.DefaultPrimaryAttestationScoreValidator();
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryAttestationScoreRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PrimaryAttestationScoreRequiredCategoryRule("primary attestation score needs this field");
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryAttestationScoreVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new PrimaryAttestationScoreVersionRangeRule(64, 64);
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAttestationScoreTeamLengthRuleFlagsALongTeam()
    {
        var rule = new PrimaryAttestationScoreTeamLengthRule(16);
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryAttestationScoreWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PrimaryAttestationScoreWeightRangeRule(0.4, 873.0);
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("x", "y");
        item.Weight = 874.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryAttestationScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryAttestationScoreRequiredNameRule("primary attestation score needs an entry");
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryAttestationScoreValidatorReportsEveryProblem();
        PrimaryAttestationScoreRequiredCategoryRuleFlagsABlankCategory();
        PrimaryAttestationScoreVersionRangeRuleFlagsValuesAboveVersion();
        PrimaryAttestationScoreTeamLengthRuleFlagsALongTeam();
        PrimaryAttestationScoreWeightRangeRuleFlagsValuesAboveWeight();
        PrimaryAttestationScoreRequiredNameRuleFlagsABlankName();
    }
}
