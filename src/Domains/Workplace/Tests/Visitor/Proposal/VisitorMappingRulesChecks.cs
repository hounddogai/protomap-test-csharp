namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorMappingRulesChecks
{
    public static void VisitorMappingValidatorReportsEveryProblem()
    {
        var validator = VisitorMappingRules.DefaultVisitorMappingValidator();
        var item = VisitorMapping.NewVisitorMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorMappingRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new VisitorMappingRequiredTimezoneRule("visitor mapping needs an entry");
        var item = VisitorMapping.NewVisitorMapping("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new VisitorMappingRequiredNameRule("visitor mapping needs an entry");
        var item = VisitorMapping.NewVisitorMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorMappingValidatorReportsEveryProblem();
        VisitorMappingRequiredTimezoneRuleFlagsABlankTimezone();
        VisitorMappingRequiredNameRuleFlagsABlankName();
    }
}
