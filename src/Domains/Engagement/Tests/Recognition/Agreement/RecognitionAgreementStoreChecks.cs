namespace Acme.Workforce.Engagement.Tests.Recognition.Agreement;

using Acme.Workforce.Engagement.Recognition.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RecognitionAgreementStoreChecks
{
    public static void MemoryRecognitionAgreementStoreFindsWhatItSaved()
    {
        var store = RecognitionAgreementStore.NewMemoryRecognitionAgreementStore();
        var item = RecognitionAgreement.NewRecognitionAgreement("code-833", "label-865");
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
        MemoryRecognitionAgreementStoreFindsWhatItSaved();
    }
}
