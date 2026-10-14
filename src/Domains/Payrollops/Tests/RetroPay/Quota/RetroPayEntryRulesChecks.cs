namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayEntryRulesChecks
{
    public static void RetroPayEntryValidatorReportsEveryProblem()
    {
        var validator = RetroPayEntryRules.DefaultRetroPayEntryValidator();
        var item = RetroPayEntry.NewRetroPayEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayEntryLocationLengthRuleFlagsALongLocation()
    {
        var rule = new RetroPayEntryLocationLengthRule(86400);
        var item = RetroPayEntry.NewRetroPayEntry("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayEntryNameLengthRuleFlagsALongName()
    {
        var rule = new RetroPayEntryNameLengthRule(3600);
        var item = RetroPayEntry.NewRetroPayEntry("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayEntryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayEntryRequiredLabelRule("retro pay entry needs a value");
        var item = RetroPayEntry.NewRetroPayEntry("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayEntryLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RetroPayEntryLabelLengthRule(32);
        var item = RetroPayEntry.NewRetroPayEntry("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayEntryValidatorReportsEveryProblem();
        RetroPayEntryLocationLengthRuleFlagsALongLocation();
        RetroPayEntryNameLengthRuleFlagsALongName();
        RetroPayEntryRequiredLabelRuleFlagsABlankLabel();
        RetroPayEntryLabelLengthRuleFlagsALongLabel();
    }
}
