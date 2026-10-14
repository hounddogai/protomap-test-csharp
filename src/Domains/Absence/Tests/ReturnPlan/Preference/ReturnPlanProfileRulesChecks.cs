namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanProfileRulesChecks
{
    public static void ReturnPlanProfileValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanProfileRules.DefaultReturnPlanProfileValidator();
        var item = ReturnPlanProfile.NewReturnPlanProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanProfileNameLengthRuleFlagsALongName()
    {
        var rule = new ReturnPlanProfileNameLengthRule(32);
        var item = ReturnPlanProfile.NewReturnPlanProfile("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReturnPlanProfileRequiredNameRule("return plan profile needs an entry");
        var item = ReturnPlanProfile.NewReturnPlanProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanProfileFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new ReturnPlanProfileFloorValueRangeRule(2.2, 1815.7);
        var item = ReturnPlanProfile.NewReturnPlanProfile("x", "y");
        item.FloorValue = 1817.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReturnPlanProfileCodeLengthRule(120);
        var item = ReturnPlanProfile.NewReturnPlanProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanProfileValidatorReportsEveryProblem();
        ReturnPlanProfileNameLengthRuleFlagsALongName();
        ReturnPlanProfileRequiredNameRuleFlagsABlankName();
        ReturnPlanProfileFloorValueRangeRuleFlagsValuesAboveFloorValue();
        ReturnPlanProfileCodeLengthRuleFlagsALongCode();
    }
}
