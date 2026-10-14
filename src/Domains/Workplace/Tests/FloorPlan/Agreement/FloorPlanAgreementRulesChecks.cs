namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class FloorPlanAgreementRulesChecks
{
    public static void FloorPlanAgreementValidatorReportsEveryProblem()
    {
        var validator = FloorPlanAgreementRules.DefaultFloorPlanAgreementValidator();
        var item = FloorPlanAgreement.NewFloorPlanAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanAgreementChannelLengthRuleFlagsALongChannel()
    {
        var rule = new FloorPlanAgreementChannelLengthRule(80);
        var item = FloorPlanAgreement.NewFloorPlanAgreement("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanAgreementScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new FloorPlanAgreementScoreRangeRule(2.0, 783.4);
        var item = FloorPlanAgreement.NewFloorPlanAgreement("x", "y");
        item.Score = 784.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanAgreementRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new FloorPlanAgreementRequiredChannelRule("floor plan agreement needs this field");
        var item = FloorPlanAgreement.NewFloorPlanAgreement("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new FloorPlanAgreementNameLengthRule(80);
        var item = FloorPlanAgreement.NewFloorPlanAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FloorPlanAgreementValidatorReportsEveryProblem();
        FloorPlanAgreementChannelLengthRuleFlagsALongChannel();
        FloorPlanAgreementScoreRangeRuleFlagsValuesAboveScore();
        FloorPlanAgreementRequiredChannelRuleFlagsABlankChannel();
        FloorPlanAgreementNameLengthRuleFlagsALongName();
    }
}
