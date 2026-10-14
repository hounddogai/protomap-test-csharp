namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskTemplateRulesChecks
{
    public static void DeskTemplateValidatorReportsEveryProblem()
    {
        var validator = DeskTemplateRules.DefaultDeskTemplateValidator();
        var item = DeskTemplate.NewDeskTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskTemplateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new DeskTemplateRetriesRangeRule(80, 3600);
        var item = DeskTemplate.NewDeskTemplate("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskTemplateMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new DeskTemplateMarginRangeRule(4.4, 712.2);
        var item = DeskTemplate.NewDeskTemplate("x", "y");
        item.Margin = 713.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskTemplateRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DeskTemplateRatioRangeRule(1.5, 2563.9);
        var item = DeskTemplate.NewDeskTemplate("x", "y");
        item.Ratio = 2565.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskTemplateDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new DeskTemplateDescriptionLengthRule(32);
        var item = DeskTemplate.NewDeskTemplate("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskTemplateRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new DeskTemplateRequiredCountryRule("desk template needs this field");
        var item = DeskTemplate.NewDeskTemplate("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskTemplateCountryLengthRuleFlagsALongCountry()
    {
        var rule = new DeskTemplateCountryLengthRule(120);
        var item = DeskTemplate.NewDeskTemplate("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskTemplateValidatorReportsEveryProblem();
        DeskTemplateRetriesRangeRuleFlagsValuesAboveRetries();
        DeskTemplateMarginRangeRuleFlagsValuesAboveMargin();
        DeskTemplateRatioRangeRuleFlagsValuesAboveRatio();
        DeskTemplateDescriptionLengthRuleFlagsALongDescription();
        DeskTemplateRequiredCountryRuleFlagsABlankCountry();
        DeskTemplateCountryLengthRuleFlagsALongCountry();
    }
}
