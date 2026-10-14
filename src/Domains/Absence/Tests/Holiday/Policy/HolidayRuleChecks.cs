namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayRuleChecks
{
    public static void HolidayRuleStartsInNew()
    {
        var item = HolidayRule.NewHolidayRule("code-152", "title-551");
        Check.Equal(item.Status, HolidayRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayRuleMovesToRunning()
    {
        var item = HolidayRule.NewHolidayRule("code-152", "title-551");
        try
        {
            item.MoveTo(HolidayRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayRuleStatus.Running, "status");
    }

    public static void HolidayRuleRejectsMovesFromFailed()
    {
        var item = HolidayRule.NewHolidayRule("code-152", "title-551");
        item.Status = HolidayRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(HolidayRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        HolidayRuleStartsInNew();
        HolidayRuleMovesToRunning();
        HolidayRuleRejectsMovesFromFailed();
    }
}
