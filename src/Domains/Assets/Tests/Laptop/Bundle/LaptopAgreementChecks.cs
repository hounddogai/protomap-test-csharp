namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopAgreementChecks
{
    public static void LaptopAgreementStartsInRequested()
    {
        var item = LaptopAgreement.NewLaptopAgreement("code-867", "label-920");
        Check.Equal(item.Status, LaptopAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopAgreementMovesToRevoked()
    {
        var item = LaptopAgreement.NewLaptopAgreement("code-867", "label-920");
        try
        {
            item.MoveTo(LaptopAgreementStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopAgreementStatus.Revoked, "status");
    }

    public static void LaptopAgreementRejectsMovesFromExpired()
    {
        var item = LaptopAgreement.NewLaptopAgreement("code-867", "label-920");
        item.Status = LaptopAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(LaptopAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LaptopAgreementStartsInRequested();
        LaptopAgreementMovesToRevoked();
        LaptopAgreementRejectsMovesFromExpired();
    }
}
