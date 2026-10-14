namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SkillProfileReservationStoreChecks
{
    public static void MemorySkillProfileReservationStoreFindsWhatItSaved()
    {
        var store = SkillProfileReservationStore.NewMemorySkillProfileReservationStore();
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemorySkillProfileReservationStoreFindsWhatItSaved();
    }
}
