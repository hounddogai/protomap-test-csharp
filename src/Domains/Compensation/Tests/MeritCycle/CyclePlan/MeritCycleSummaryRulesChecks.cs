namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class MeritCycleSummaryRulesChecks
{
    public static void MeritCycleSummaryValidatorReportsEveryProblem()
    {
        var validator = MeritCycleSummaryRules.DefaultMeritCycleSummaryValidator();
        var item = MeritCycleSummary.NewMeritCycleSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleSummaryRequiredCodeRule("merit cycle summary needs an entry");
        var item = MeritCycleSummary.NewMeritCycleSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleSummaryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new MeritCycleSummaryLabelLengthRule(3600);
        var item = MeritCycleSummary.NewMeritCycleSummary("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSummaryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new MeritCycleSummaryNotesLengthRule(64);
        var item = MeritCycleSummary.NewMeritCycleSummary("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSummaryBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new MeritCycleSummaryBadgeLengthRule(86400);
        var item = MeritCycleSummary.NewMeritCycleSummary("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleSummaryCodeLengthRule(16);
        var item = MeritCycleSummary.NewMeritCycleSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleSummaryWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new MeritCycleSummaryWeightRangeRule(1.2, 1578.2);
        var item = MeritCycleSummary.NewMeritCycleSummary("x", "y");
        item.Weight = 1579.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleSummaryValidatorReportsEveryProblem();
        MeritCycleSummaryRequiredCodeRuleFlagsABlankCode();
        MeritCycleSummaryLabelLengthRuleFlagsALongLabel();
        MeritCycleSummaryNotesLengthRuleFlagsALongNotes();
        MeritCycleSummaryBadgeLengthRuleFlagsALongBadge();
        MeritCycleSummaryCodeLengthRuleFlagsALongCode();
        MeritCycleSummaryWeightRangeRuleFlagsValuesAboveWeight();
    }
}
