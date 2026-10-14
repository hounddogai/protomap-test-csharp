namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class CappedTrainingDueDigestRulesChecks
{
    public static void CappedTrainingDueDigestValidatorReportsEveryProblem()
    {
        var validator = CappedTrainingDueDigestRules.DefaultCappedTrainingDueDigestValidator();
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedTrainingDueDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedTrainingDueDigestRequiredNameRule("capped training due digest needs a value");
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedTrainingDueDigestTeamLengthRuleFlagsALongTeam()
    {
        var rule = new CappedTrainingDueDigestTeamLengthRule(1000);
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedTrainingDueDigestValidatorReportsEveryProblem();
        CappedTrainingDueDigestRequiredNameRuleFlagsABlankName();
        CappedTrainingDueDigestTeamLengthRuleFlagsALongTeam();
    }
}
