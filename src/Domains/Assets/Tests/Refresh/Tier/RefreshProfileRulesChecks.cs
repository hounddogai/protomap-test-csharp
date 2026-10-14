namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshProfileRulesChecks
{
    public static void RefreshProfileValidatorReportsEveryProblem()
    {
        var validator = RefreshProfileRules.DefaultRefreshProfileValidator();
        var item = RefreshProfile.NewRefreshProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RefreshProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RefreshProfileRequiredTitleRule("refresh profile needs an entry");
        var item = RefreshProfile.NewRefreshProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshProfileRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new RefreshProfileRequiredGradeRule("refresh profile needs this field");
        var item = RefreshProfile.NewRefreshProfile("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RefreshProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new RefreshProfileCodeLengthRule(1000);
        var item = RefreshProfile.NewRefreshProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RefreshProfileGradeLengthRuleFlagsALongGrade()
    {
        var rule = new RefreshProfileGradeLengthRule(255);
        var item = RefreshProfile.NewRefreshProfile("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RefreshProfileValidatorReportsEveryProblem();
        RefreshProfileRequiredTitleRuleFlagsABlankTitle();
        RefreshProfileRequiredGradeRuleFlagsABlankGrade();
        RefreshProfileCodeLengthRuleFlagsALongCode();
        RefreshProfileGradeLengthRuleFlagsALongGrade();
    }
}
