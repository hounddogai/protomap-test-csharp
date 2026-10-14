namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayPolicyChecks
{
    public static void HolidayPolicyStartsInCreated()
    {
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
        Check.Equal(item.Status, HolidayPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayPolicyMovesToValidated()
    {
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
        try
        {
            item.MoveTo(HolidayPolicyStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayPolicyStatus.Validated, "status");
    }

    public static void HolidayPolicyRejectsMovesFromRetired()
    {
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
        item.Status = HolidayPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(HolidayPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        HolidayPolicyStartsInCreated();
        HolidayPolicyMovesToValidated();
        HolidayPolicyRejectsMovesFromRetired();
    }
}
