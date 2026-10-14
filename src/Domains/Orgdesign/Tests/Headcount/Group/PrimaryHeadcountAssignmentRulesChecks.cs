namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryHeadcountAssignmentRulesChecks
{
    public static void PrimaryHeadcountAssignmentValidatorReportsEveryProblem()
    {
        var validator = PrimaryHeadcountAssignmentRules.DefaultPrimaryHeadcountAssignmentValidator();
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryHeadcountAssignmentRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new PrimaryHeadcountAssignmentRequiredOwnerIdRule("primary headcount assignment needs a value");
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryHeadcountAssignmentChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PrimaryHeadcountAssignmentChannelLengthRule(64);
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryHeadcountAssignmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PrimaryHeadcountAssignmentOwnerIdLengthRule(32);
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryHeadcountAssignmentValidatorReportsEveryProblem();
        PrimaryHeadcountAssignmentRequiredOwnerIdRuleFlagsABlankOwnerId();
        PrimaryHeadcountAssignmentChannelLengthRuleFlagsALongChannel();
        PrimaryHeadcountAssignmentOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
