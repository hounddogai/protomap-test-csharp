namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CareerPathWindowStoreChecks
{
    public static void MemoryCareerPathWindowStoreFindsWhatItSaved()
    {
        var store = CareerPathWindowStore.NewMemoryCareerPathWindowStore();
        var item = CareerPathWindow.NewCareerPathWindow("code-195", "title-626");
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
        MemoryCareerPathWindowStoreFindsWhatItSaved();
    }
}
