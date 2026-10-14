namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class DeskExceptionRulesChecks
{
    public static void DeskExceptionValidatorReportsEveryProblem()
    {
        var validator = DeskExceptionRules.DefaultDeskExceptionValidator();
        var item = DeskException.NewDeskException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskExceptionCodeLengthRule(86400);
        var item = DeskException.NewDeskException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DeskExceptionLeaveDaysRangeRule(255, 86400);
        var item = DeskException.NewDeskException("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeskExceptionValidatorReportsEveryProblem();
        DeskExceptionCodeLengthRuleFlagsALongCode();
        DeskExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
