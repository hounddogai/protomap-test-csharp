namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyRatePreferenceChecks
{
    public static void LegacyRatePreferenceStartsInCreated()
    {
        var item = LegacyRatePreference.NewLegacyRatePreference("code-596", "title-600");
        Check.Equal(item.Status, LegacyRatePreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRatePreferenceMovesToValidated()
    {
        var item = LegacyRatePreference.NewLegacyRatePreference("code-596", "title-600");
        try
        {
            item.MoveTo(LegacyRatePreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRatePreferenceStatus.Validated, "status");
    }

    public static void LegacyRatePreferenceRejectsMovesFromRetired()
    {
        var item = LegacyRatePreference.NewLegacyRatePreference("code-596", "title-600");
        item.Status = LegacyRatePreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyRatePreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyRatePreferenceStartsInCreated();
        LegacyRatePreferenceMovesToValidated();
        LegacyRatePreferenceRejectsMovesFromRetired();
    }
}
