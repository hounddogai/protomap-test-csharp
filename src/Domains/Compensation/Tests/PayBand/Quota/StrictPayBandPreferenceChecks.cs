namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictPayBandPreferenceChecks
{
    public static void StrictPayBandPreferenceStartsInCreated()
    {
        var item = StrictPayBandPreference.NewStrictPayBandPreference("code-527", "name-204");
        Check.Equal(item.Status, StrictPayBandPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPayBandPreferenceMovesToValidated()
    {
        var item = StrictPayBandPreference.NewStrictPayBandPreference("code-527", "name-204");
        try
        {
            item.MoveTo(StrictPayBandPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPayBandPreferenceStatus.Validated, "status");
    }

    public static void StrictPayBandPreferenceRejectsMovesFromRetired()
    {
        var item = StrictPayBandPreference.NewStrictPayBandPreference("code-527", "name-204");
        item.Status = StrictPayBandPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictPayBandPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictPayBandPreferenceStartsInCreated();
        StrictPayBandPreferenceMovesToValidated();
        StrictPayBandPreferenceRejectsMovesFromRetired();
    }
}
