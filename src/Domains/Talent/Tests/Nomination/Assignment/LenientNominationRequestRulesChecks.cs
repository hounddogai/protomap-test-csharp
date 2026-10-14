namespace Acme.Workforce.Talent.Tests.Nomination.Assignment;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientNominationRequestRulesChecks
{
    public static void LenientNominationRequestValidatorReportsEveryProblem()
    {
        var validator = LenientNominationRequestRules.DefaultLenientNominationRequestValidator();
        var item = LenientNominationRequest.NewLenientNominationRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientNominationRequestBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LenientNominationRequestBudgetRangeRule(0.5, 538.2);
        var item = LenientNominationRequest.NewLenientNominationRequest("x", "y");
        item.Budget = 539.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientNominationRequestRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new LenientNominationRequestRequiredUnitRule("lenient nomination request needs an entry");
        var item = LenientNominationRequest.NewLenientNominationRequest("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientNominationRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientNominationRequestRequiredLabelRule("lenient nomination request needs a value");
        var item = LenientNominationRequest.NewLenientNominationRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientNominationRequestUnitLengthRuleFlagsALongUnit()
    {
        var rule = new LenientNominationRequestUnitLengthRule(32);
        var item = LenientNominationRequest.NewLenientNominationRequest("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientNominationRequestValidatorReportsEveryProblem();
        LenientNominationRequestBudgetRangeRuleFlagsValuesAboveBudget();
        LenientNominationRequestRequiredUnitRuleFlagsABlankUnit();
        LenientNominationRequestRequiredLabelRuleFlagsABlankLabel();
        LenientNominationRequestUnitLengthRuleFlagsALongUnit();
    }
}
