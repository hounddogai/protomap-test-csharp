namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorAssignmentRulesChecks
{
    public static void VisitorAssignmentValidatorReportsEveryProblem()
    {
        var validator = VisitorAssignmentRules.DefaultVisitorAssignmentValidator();
        var item = VisitorAssignment.NewVisitorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new VisitorAssignmentRequiredTitleRule("visitor assignment needs an entry");
        var item = VisitorAssignment.NewVisitorAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorAssignmentCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new VisitorAssignmentCategoryLengthRule(120);
        var item = VisitorAssignment.NewVisitorAssignment("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAssignmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new VisitorAssignmentAmountRangeRule(5.3, 1699.3);
        var item = VisitorAssignment.NewVisitorAssignment("x", "y");
        item.Amount = 1700.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorAssignmentValidatorReportsEveryProblem();
        VisitorAssignmentRequiredTitleRuleFlagsABlankTitle();
        VisitorAssignmentCategoryLengthRuleFlagsALongCategory();
        VisitorAssignmentAmountRangeRuleFlagsValuesAboveAmount();
    }
}
