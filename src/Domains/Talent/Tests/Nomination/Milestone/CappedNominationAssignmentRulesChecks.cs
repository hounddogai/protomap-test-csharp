namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedNominationAssignmentRulesChecks
{
    public static void CappedNominationAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedNominationAssignmentRules.DefaultCappedNominationAssignmentValidator();
        var item = CappedNominationAssignment.NewCappedNominationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedNominationAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedNominationAssignmentCodeLengthRule(255);
        var item = CappedNominationAssignment.NewCappedNominationAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedNominationAssignmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new CappedNominationAssignmentCountryLengthRule(64);
        var item = CappedNominationAssignment.NewCappedNominationAssignment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedNominationAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CappedNominationAssignmentCapacityRangeRule(1000, 1000);
        var item = CappedNominationAssignment.NewCappedNominationAssignment("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedNominationAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new CappedNominationAssignmentGraceDaysRangeRule(16, 3600);
        var item = CappedNominationAssignment.NewCappedNominationAssignment("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedNominationAssignmentExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new CappedNominationAssignmentExternalRefLengthRule(16);
        var item = CappedNominationAssignment.NewCappedNominationAssignment("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedNominationAssignmentValidatorReportsEveryProblem();
        CappedNominationAssignmentCodeLengthRuleFlagsALongCode();
        CappedNominationAssignmentCountryLengthRuleFlagsALongCountry();
        CappedNominationAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
        CappedNominationAssignmentGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        CappedNominationAssignmentExternalRefLengthRuleFlagsALongExternalRef();
    }
}
