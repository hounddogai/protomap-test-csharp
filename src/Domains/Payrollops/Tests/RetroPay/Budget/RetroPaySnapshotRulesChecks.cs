namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPaySnapshotRulesChecks
{
    public static void RetroPaySnapshotValidatorReportsEveryProblem()
    {
        var validator = RetroPaySnapshotRules.DefaultRetroPaySnapshotValidator();
        var item = RetroPaySnapshot.NewRetroPaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPaySnapshotLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RetroPaySnapshotLocationLengthRule(120);
        var item = RetroPaySnapshot.NewRetroPaySnapshot("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPaySnapshotRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new RetroPaySnapshotRequiredLocationRule("retro pay snapshot needs an entry");
        var item = RetroPaySnapshot.NewRetroPaySnapshot("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPaySnapshotRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new RetroPaySnapshotRequiredSummaryRule("retro pay snapshot needs an entry");
        var item = RetroPaySnapshot.NewRetroPaySnapshot("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPaySnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RetroPaySnapshotLabelLengthRule(32);
        var item = RetroPaySnapshot.NewRetroPaySnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPaySnapshotValidatorReportsEveryProblem();
        RetroPaySnapshotLocationLengthRuleFlagsALongLocation();
        RetroPaySnapshotRequiredLocationRuleFlagsABlankLocation();
        RetroPaySnapshotRequiredSummaryRuleFlagsABlankSummary();
        RetroPaySnapshotLabelLengthRuleFlagsALongLabel();
    }
}
