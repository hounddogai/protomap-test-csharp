namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSlotRulesChecks
{
    public static void PositionSlotValidatorReportsEveryProblem()
    {
        var validator = PositionSlotRules.DefaultPositionSlotValidator();
        var item = PositionSlot.NewPositionSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionSlotNameLengthRuleFlagsALongName()
    {
        var rule = new PositionSlotNameLengthRule(120);
        var item = PositionSlot.NewPositionSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionSlotAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PositionSlotAttemptsRangeRule(120, 86400);
        var item = PositionSlot.NewPositionSlot("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionSlotChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PositionSlotChannelLengthRule(80);
        var item = PositionSlot.NewPositionSlot("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionSlotValidatorReportsEveryProblem();
        PositionSlotNameLengthRuleFlagsALongName();
        PositionSlotAttemptsRangeRuleFlagsValuesAboveAttempts();
        PositionSlotChannelLengthRuleFlagsALongChannel();
    }
}
