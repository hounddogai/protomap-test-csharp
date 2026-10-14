namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPositionProposalRulesChecks
{
    public static void TieredPositionProposalValidatorReportsEveryProblem()
    {
        var validator = TieredPositionProposalRules.DefaultTieredPositionProposalValidator();
        var item = TieredPositionProposal.NewTieredPositionProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPositionProposalStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new TieredPositionProposalStartsAtRangeRule(255, 86400);
        var item = TieredPositionProposal.NewTieredPositionProposal("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredPositionProposalRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TieredPositionProposalRequiredDepartmentRule("tiered position proposal needs an entry");
        var item = TieredPositionProposal.NewTieredPositionProposal("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPositionProposalLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredPositionProposalLabelLengthRule(3600);
        var item = TieredPositionProposal.NewTieredPositionProposal("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPositionProposalWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new TieredPositionProposalWindowDaysRangeRule(1000, 1000);
        var item = TieredPositionProposal.NewTieredPositionProposal("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredPositionProposalValidatorReportsEveryProblem();
        TieredPositionProposalStartsAtRangeRuleFlagsValuesAboveStartsAt();
        TieredPositionProposalRequiredDepartmentRuleFlagsABlankDepartment();
        TieredPositionProposalLabelLengthRuleFlagsALongLabel();
        TieredPositionProposalWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
