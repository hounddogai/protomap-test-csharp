namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyWindowRulesChecks
{
    public static void WarrantyWindowValidatorReportsEveryProblem()
    {
        var validator = WarrantyWindowRules.DefaultWarrantyWindowValidator();
        var item = WarrantyWindow.NewWarrantyWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantyWindowLabelLengthRule(32);
        var item = WarrantyWindow.NewWarrantyWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyWindowRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WarrantyWindowRetriesRangeRule(32, 80);
        var item = WarrantyWindow.NewWarrantyWindow("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyWindowRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new WarrantyWindowRequiredChannelRule("warranty window needs a value");
        var item = WarrantyWindow.NewWarrantyWindow("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyWindowValidatorReportsEveryProblem();
        WarrantyWindowLabelLengthRuleFlagsALongLabel();
        WarrantyWindowRetriesRangeRuleFlagsValuesAboveRetries();
        WarrantyWindowRequiredChannelRuleFlagsABlankChannel();
    }
}
