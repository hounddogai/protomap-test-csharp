namespace Acme.Workforce.Orgdesign.Tests.Scenario.Quota;

using Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScenarioProposalRulesChecks
{
    public static void ScenarioProposalValidatorReportsEveryProblem()
    {
        var validator = ScenarioProposalRules.DefaultScenarioProposalValidator();
        var item = ScenarioProposal.NewScenarioProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScenarioProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ScenarioProposalCreatedAtRangeRule(16, 86400);
        var item = ScenarioProposal.NewScenarioProposal("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScenarioProposalJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ScenarioProposalJobCodeLengthRule(16);
        var item = ScenarioProposal.NewScenarioProposal("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScenarioProposalCodeLengthRule(3600);
        var item = ScenarioProposal.NewScenarioProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScenarioProposalTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScenarioProposalTitleLengthRule(1000);
        var item = ScenarioProposal.NewScenarioProposal("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScenarioProposalValidatorReportsEveryProblem();
        ScenarioProposalCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        ScenarioProposalJobCodeLengthRuleFlagsALongJobCode();
        ScenarioProposalCodeLengthRuleFlagsALongCode();
        ScenarioProposalTitleLengthRuleFlagsALongTitle();
    }
}
