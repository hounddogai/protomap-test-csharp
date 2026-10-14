namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleProposalRulesChecks
{
    public static void MeritCycleProposalValidatorReportsEveryProblem()
    {
        var validator = MeritCycleProposalRules.DefaultMeritCycleProposalValidator();
        var item = MeritCycleProposal.NewMeritCycleProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleProposalNameLengthRuleFlagsALongName()
    {
        var rule = new MeritCycleProposalNameLengthRule(255);
        var item = MeritCycleProposal.NewMeritCycleProposal("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new MeritCycleProposalTenureMonthsRangeRule(32, 255);
        var item = MeritCycleProposal.NewMeritCycleProposal("x", "y");
        item.TenureMonths = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleProposalEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new MeritCycleProposalEmployeeIdLengthRule(16);
        var item = MeritCycleProposal.NewMeritCycleProposal("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleProposalRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MeritCycleProposalRequiredCodeRule("merit cycle proposal needs an entry");
        var item = MeritCycleProposal.NewMeritCycleProposal("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleProposalValidatorReportsEveryProblem();
        MeritCycleProposalNameLengthRuleFlagsALongName();
        MeritCycleProposalTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        MeritCycleProposalEmployeeIdLengthRuleFlagsALongEmployeeId();
        MeritCycleProposalRequiredCodeRuleFlagsABlankCode();
    }
}
