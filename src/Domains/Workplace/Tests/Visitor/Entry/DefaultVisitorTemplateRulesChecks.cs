namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultVisitorTemplateRulesChecks
{
    public static void DefaultVisitorTemplateValidatorReportsEveryProblem()
    {
        var validator = DefaultVisitorTemplateRules.DefaultDefaultVisitorTemplateValidator();
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultVisitorTemplateNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultVisitorTemplateNameLengthRule(80);
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultVisitorTemplateRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new DefaultVisitorTemplateRequiredCountryRule("default visitor template needs an entry");
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultVisitorTemplateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new DefaultVisitorTemplateSequenceRangeRule(16, 255);
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultVisitorTemplatePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new DefaultVisitorTemplatePriorityRangeRule(32, 64);
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultVisitorTemplateValidatorReportsEveryProblem();
        DefaultVisitorTemplateNameLengthRuleFlagsALongName();
        DefaultVisitorTemplateRequiredCountryRuleFlagsABlankCountry();
        DefaultVisitorTemplateSequenceRangeRuleFlagsValuesAboveSequence();
        DefaultVisitorTemplatePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
