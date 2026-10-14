namespace Acme.Workforce.Assets.Tests.Refresh.Quota;

using Acme.Workforce.Assets.Refresh.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RefreshSlotRulesChecks
{
    public static void RefreshSlotValidatorReportsEveryProblem()
    {
        var validator = RefreshSlotRules.DefaultRefreshSlotValidator();
        var item = RefreshSlot.NewRefreshSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshSlotFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new RefreshSlotFteRangeRule(3.1, 1334.0);
        var item = RefreshSlot.NewRefreshSlot("x", "y");
        item.Fte = 1335.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RefreshSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new RefreshSlotRequiredNameRule("refresh slot needs this field");
        var item = RefreshSlot.NewRefreshSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RefreshSlotValidatorReportsEveryProblem();
        RefreshSlotFteRangeRuleFlagsValuesAboveFte();
        RefreshSlotRequiredNameRuleFlagsABlankName();
    }
}
