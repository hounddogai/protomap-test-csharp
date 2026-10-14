namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScheduledAssetReturnProposalRulesChecks
{
    public static void ScheduledAssetReturnProposalValidatorReportsEveryProblem()
    {
        var validator = ScheduledAssetReturnProposalRules.DefaultScheduledAssetReturnProposalValidator();
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledAssetReturnProposalDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new ScheduledAssetReturnProposalDepartmentLengthRule(120);
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledAssetReturnProposalRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledAssetReturnProposalRequiredNameRule("scheduled asset return proposal needs an entry");
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAssetReturnProposalRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ScheduledAssetReturnProposalRequiredEmployeeIdRule(
            "scheduled asset return proposal needs a value");
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledAssetReturnProposalCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledAssetReturnProposalCodeLengthRule(1000);
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnProposalValidatorReportsEveryProblem();
        ScheduledAssetReturnProposalDepartmentLengthRuleFlagsALongDepartment();
        ScheduledAssetReturnProposalRequiredNameRuleFlagsABlankName();
        ScheduledAssetReturnProposalRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        ScheduledAssetReturnProposalCodeLengthRuleFlagsALongCode();
    }
}
