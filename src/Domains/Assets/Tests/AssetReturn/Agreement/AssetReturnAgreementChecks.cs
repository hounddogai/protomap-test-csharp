namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnAgreementChecks
{
    public static void AssetReturnAgreementStartsInCreated()
    {
        var item = AssetReturnAgreement.NewAssetReturnAgreement("code-436", "name-516");
        Check.Equal(item.Status, AssetReturnAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnAgreementMovesToPublished()
    {
        var item = AssetReturnAgreement.NewAssetReturnAgreement("code-436", "name-516");
        try
        {
            item.MoveTo(AssetReturnAgreementStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnAgreementStatus.Published, "status");
    }

    public static void AssetReturnAgreementRejectsMovesFromRetired()
    {
        var item = AssetReturnAgreement.NewAssetReturnAgreement("code-436", "name-516");
        item.Status = AssetReturnAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(AssetReturnAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssetReturnAgreementStartsInCreated();
        AssetReturnAgreementMovesToPublished();
        AssetReturnAgreementRejectsMovesFromRetired();
    }
}
