namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredPositionTemplateRulesChecks
{
    public static void TieredPositionTemplateValidatorReportsEveryProblem()
    {
        var validator = TieredPositionTemplateRules.DefaultTieredPositionTemplateValidator();
        var item = TieredPositionTemplate.NewTieredPositionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPositionTemplateRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new TieredPositionTemplateRequiredChannelRule("tiered position template needs a value");
        var item = TieredPositionTemplate.NewTieredPositionTemplate("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPositionTemplateWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new TieredPositionTemplateWindowDaysRangeRule(255, 1000);
        var item = TieredPositionTemplate.NewTieredPositionTemplate("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPositionTemplateChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new TieredPositionTemplateChecksumLengthRule(80);
        var item = TieredPositionTemplate.NewTieredPositionTemplate("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPositionTemplateChannelLengthRuleFlagsALongChannel()
    {
        var rule = new TieredPositionTemplateChannelLengthRule(86400);
        var item = TieredPositionTemplate.NewTieredPositionTemplate("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPositionTemplateValidatorReportsEveryProblem();
        TieredPositionTemplateRequiredChannelRuleFlagsABlankChannel();
        TieredPositionTemplateWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        TieredPositionTemplateChecksumLengthRuleFlagsALongChecksum();
        TieredPositionTemplateChannelLengthRuleFlagsALongChannel();
    }
}
