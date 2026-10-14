namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LegacyLeaveRequestPreferenceChecks
{
    public static void LegacyLeaveRequestPreferenceStartsInCreated()
    {
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("code-598", "label-782");
        Check.Equal(item.Status, LegacyLeaveRequestPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLeaveRequestPreferenceMovesToValidated()
    {
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("code-598", "label-782");
        try
        {
            item.MoveTo(LegacyLeaveRequestPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLeaveRequestPreferenceStatus.Validated, "status");
    }

    public static void LegacyLeaveRequestPreferenceRejectsMovesFromRetired()
    {
        var item = LegacyLeaveRequestPreference.NewLegacyLeaveRequestPreference("code-598", "label-782");
        item.Status = LegacyLeaveRequestPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyLeaveRequestPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestPreferenceStartsInCreated();
        LegacyLeaveRequestPreferenceMovesToValidated();
        LegacyLeaveRequestPreferenceRejectsMovesFromRetired();
    }
}
