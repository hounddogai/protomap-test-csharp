namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RecognitionRuleStoreChecks
{
    public static void MemoryRecognitionRuleStoreFindsWhatItSaved()
    {
        var store = RecognitionRuleStore.NewMemoryRecognitionRuleStore();
        var item = RecognitionRule.NewRecognitionRule("code-801", "title-973");
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
        MemoryRecognitionRuleStoreFindsWhatItSaved();
    }
}
