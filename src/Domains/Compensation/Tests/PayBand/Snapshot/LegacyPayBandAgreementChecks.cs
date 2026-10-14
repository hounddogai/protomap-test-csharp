namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyPayBandAgreementChecks
{
    public static void LegacyPayBandAgreementStartsInCreated()
    {
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("code-235", "label-214");
        Check.Equal(item.Status, LegacyPayBandAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayBandAgreementMovesToPublished()
    {
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("code-235", "label-214");
        try
        {
            item.MoveTo(LegacyPayBandAgreementStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayBandAgreementStatus.Published, "status");
    }

    public static void LegacyPayBandAgreementRejectsMovesFromRetired()
    {
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("code-235", "label-214");
        item.Status = LegacyPayBandAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyPayBandAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyPayBandAgreementStartsInCreated();
        LegacyPayBandAgreementMovesToPublished();
        LegacyPayBandAgreementRejectsMovesFromRetired();
    }
}
