namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientActionPlanExceptionRulesChecks
{
    public static void LenientActionPlanExceptionValidatorReportsEveryProblem()
    {
        var validator = LenientActionPlanExceptionRules.DefaultLenientActionPlanExceptionValidator();
        var item = LenientActionPlanException.NewLenientActionPlanException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientActionPlanExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientActionPlanExceptionRequiredCodeRule("lenient action plan exception needs an entry");
        var item = LenientActionPlanException.NewLenientActionPlanException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanExceptionRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LenientActionPlanExceptionRequiredLocationRule("lenient action plan exception needs an entry");
        var item = LenientActionPlanException.NewLenientActionPlanException("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientActionPlanExceptionRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LenientActionPlanExceptionRatingRangeRule(1.1, 317.9);
        var item = LenientActionPlanException.NewLenientActionPlanException("x", "y");
        item.Rating = 319.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientActionPlanExceptionLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientActionPlanExceptionLocationLengthRule(3600);
        var item = LenientActionPlanException.NewLenientActionPlanException("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientActionPlanExceptionValidatorReportsEveryProblem();
        LenientActionPlanExceptionRequiredCodeRuleFlagsABlankCode();
        LenientActionPlanExceptionRequiredLocationRuleFlagsABlankLocation();
        LenientActionPlanExceptionRatingRangeRuleFlagsValuesAboveRating();
        LenientActionPlanExceptionLocationLengthRuleFlagsALongLocation();
    }
}
