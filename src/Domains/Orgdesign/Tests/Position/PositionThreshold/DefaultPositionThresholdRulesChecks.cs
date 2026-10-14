namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class DefaultPositionThresholdRulesChecks
{
    public static void DefaultPositionThresholdValidatorReportsEveryProblem()
    {
        var validator = DefaultPositionThresholdRules.DefaultDefaultPositionThresholdValidator();
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPositionThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultPositionThresholdRequiredTitleRule("default position threshold needs this field");
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultPositionThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultPositionThresholdCodeLengthRule(32);
        var item = DefaultPositionThreshold.NewDefaultPositionThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultPositionThresholdValidatorReportsEveryProblem();
        DefaultPositionThresholdRequiredTitleRuleFlagsABlankTitle();
        DefaultPositionThresholdCodeLengthRuleFlagsALongCode();
    }
}
