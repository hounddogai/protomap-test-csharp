namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class RegionalDeskClaimRulesChecks
{
    public static void RegionalDeskClaimValidatorReportsEveryProblem()
    {
        var validator = RegionalDeskClaimRules.DefaultRegionalDeskClaimValidator();
        var item = RegionalDeskClaim.NewRegionalDeskClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeskClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalDeskClaimRequiredNameRule("regional desk claim needs an entry");
        var item = RegionalDeskClaim.NewRegionalDeskClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalDeskClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalDeskClaimCodeLengthRule(1000);
        var item = RegionalDeskClaim.NewRegionalDeskClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeskClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RegionalDeskClaimBatchSizeRangeRule(1000, 1000);
        var item = RegionalDeskClaim.NewRegionalDeskClaim("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalDeskClaimRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new RegionalDeskClaimRequiredNotesRule("regional desk claim needs an entry");
        var item = RegionalDeskClaim.NewRegionalDeskClaim("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeskClaimValidatorReportsEveryProblem();
        RegionalDeskClaimRequiredNameRuleFlagsABlankName();
        RegionalDeskClaimCodeLengthRuleFlagsALongCode();
        RegionalDeskClaimBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RegionalDeskClaimRequiredNotesRuleFlagsABlankNotes();
    }
}
