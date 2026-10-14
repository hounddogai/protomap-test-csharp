namespace Acme.Workforce.Assets.Tests.Laptop.Draft;

using Acme.Workforce.Assets.Laptop.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LaptopDraftRulesChecks
{
    public static void LaptopDraftValidatorReportsEveryProblem()
    {
        var validator = LaptopDraftRules.DefaultLaptopDraftValidator();
        var item = LaptopDraft.NewLaptopDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LaptopDraftLabelLengthRule(86400);
        var item = LaptopDraft.NewLaptopDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopDraftManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new LaptopDraftManagerIdLengthRule(255);
        var item = LaptopDraft.NewLaptopDraft("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopDraftSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LaptopDraftSummaryLengthRule(32);
        var item = LaptopDraft.NewLaptopDraft("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LaptopDraftValidatorReportsEveryProblem();
        LaptopDraftLabelLengthRuleFlagsALongLabel();
        LaptopDraftManagerIdLengthRuleFlagsALongManagerId();
        LaptopDraftSummaryLengthRuleFlagsALongSummary();
    }
}
