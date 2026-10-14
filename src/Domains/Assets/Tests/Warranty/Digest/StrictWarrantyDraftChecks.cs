namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StrictWarrantyDraftChecks
{
    public static void StrictWarrantyDraftStartsInNew()
    {
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("code-374", "name-540");
        Check.Equal(item.Status, StrictWarrantyDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictWarrantyDraftMovesToFailed()
    {
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("code-374", "name-540");
        try
        {
            item.MoveTo(StrictWarrantyDraftStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictWarrantyDraftStatus.Failed, "status");
    }

    public static void StrictWarrantyDraftRejectsMovesFromFailed()
    {
        var item = StrictWarrantyDraft.NewStrictWarrantyDraft("code-374", "name-540");
        item.Status = StrictWarrantyDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictWarrantyDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictWarrantyDraftStartsInNew();
        StrictWarrantyDraftMovesToFailed();
        StrictWarrantyDraftRejectsMovesFromFailed();
    }
}
