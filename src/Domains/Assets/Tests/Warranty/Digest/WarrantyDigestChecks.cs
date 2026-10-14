namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class WarrantyDigestChecks
{
    public static void WarrantyDigestStartsInCreated()
    {
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
        Check.Equal(item.Status, WarrantyDigestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyDigestMovesToValidated()
    {
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
        try
        {
            item.MoveTo(WarrantyDigestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyDigestStatus.Validated, "status");
    }

    public static void WarrantyDigestRejectsMovesFromRetired()
    {
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
        item.Status = WarrantyDigestStatus.Retired;
        Check.Fails(() => item.MoveTo(WarrantyDigestStatus.Created), "move");
    }

    public static void RunAll()
    {
        WarrantyDigestStartsInCreated();
        WarrantyDigestMovesToValidated();
        WarrantyDigestRejectsMovesFromRetired();
    }
}
