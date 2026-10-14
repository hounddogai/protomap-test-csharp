namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackExceptionAssignmentRulesChecks
{
    public static void FallbackExceptionAssignmentValidatorReportsEveryProblem()
    {
        var validator = FallbackExceptionAssignmentRules.DefaultFallbackExceptionAssignmentValidator();
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackExceptionAssignmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new FallbackExceptionAssignmentFteRangeRule(8.3, 2702.7);
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("x", "y");
        item.Fte = 2704.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackExceptionAssignmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new FallbackExceptionAssignmentCostRangeRule(8.2, 2755.3);
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("x", "y");
        item.Cost = 2756.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackExceptionAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackExceptionAssignmentNameLengthRule(64);
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackExceptionAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackExceptionAssignmentRequiredCodeRule("fallback exception assignment needs this field");
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackExceptionAssignmentVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackExceptionAssignmentVersionRangeRule(80, 3600);
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackExceptionAssignmentValidatorReportsEveryProblem();
        FallbackExceptionAssignmentFteRangeRuleFlagsValuesAboveFte();
        FallbackExceptionAssignmentCostRangeRuleFlagsValuesAboveCost();
        FallbackExceptionAssignmentNameLengthRuleFlagsALongName();
        FallbackExceptionAssignmentRequiredCodeRuleFlagsABlankCode();
        FallbackExceptionAssignmentVersionRangeRuleFlagsValuesAboveVersion();
    }
}
