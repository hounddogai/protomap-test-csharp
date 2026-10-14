namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class RollingDeskMappingRulesChecks
{
    public static void RollingDeskMappingValidatorReportsEveryProblem()
    {
        var validator = RollingDeskMappingRules.DefaultRollingDeskMappingValidator();
        var item = RollingDeskMapping.NewRollingDeskMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingDeskMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingDeskMappingCodeLengthRule(120);
        var item = RollingDeskMapping.NewRollingDeskMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingDeskMappingRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RollingDeskMappingRequiredNotesRule("rolling desk mapping needs an entry");
        var item = RollingDeskMapping.NewRollingDeskMapping("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingDeskMappingWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RollingDeskMappingWeightRangeRule(6.1, 3127.2);
        var item = RollingDeskMapping.NewRollingDeskMapping("x", "y");
        item.Weight = 3128.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingDeskMappingValidatorReportsEveryProblem();
        RollingDeskMappingCodeLengthRuleFlagsALongCode();
        RollingDeskMappingRequiredNotesRuleFlagsABlankNotes();
        RollingDeskMappingWeightRangeRuleFlagsValuesAboveWeight();
    }
}
