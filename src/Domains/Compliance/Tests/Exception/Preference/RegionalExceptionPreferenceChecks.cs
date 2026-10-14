namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalExceptionPreferenceChecks
{
    public static void RegionalExceptionPreferenceStartsInCreated()
    {
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
        Check.Equal(item.Status, RegionalExceptionPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalExceptionPreferenceMovesToValidated()
    {
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
        try
        {
            item.MoveTo(RegionalExceptionPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalExceptionPreferenceStatus.Validated, "status");
    }

    public static void RegionalExceptionPreferenceRejectsMovesFromRetired()
    {
        var item = RegionalExceptionPreference.NewRegionalExceptionPreference("code-623", "label-904");
        item.Status = RegionalExceptionPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalExceptionPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalExceptionPreferenceStartsInCreated();
        RegionalExceptionPreferenceMovesToValidated();
        RegionalExceptionPreferenceRejectsMovesFromRetired();
    }
}
