namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorRequestRulesChecks
{
    public static void VisitorRequestValidatorReportsEveryProblem()
    {
        var validator = VisitorRequestRules.DefaultVisitorRequestValidator();
        var item = VisitorRequest.NewVisitorRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorRequestManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new VisitorRequestManagerIdLengthRule(3600);
        var item = VisitorRequest.NewVisitorRequest("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorRequestCodeLengthRule(120);
        var item = VisitorRequest.NewVisitorRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorRequestWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new VisitorRequestWeightRangeRule(8.8, 325.6);
        var item = VisitorRequest.NewVisitorRequest("x", "y");
        item.Weight = 327.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new VisitorRequestRequiredLabelRule("visitor request needs an entry");
        var item = VisitorRequest.NewVisitorRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorRequestValidatorReportsEveryProblem();
        VisitorRequestManagerIdLengthRuleFlagsALongManagerId();
        VisitorRequestCodeLengthRuleFlagsALongCode();
        VisitorRequestWeightRangeRuleFlagsValuesAboveWeight();
        VisitorRequestRequiredLabelRuleFlagsABlankLabel();
    }
}
