namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LenientLaptopAgreementChecks
{
    public static void LenientLaptopAgreementStartsInNew()
    {
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("code-740", "name-498");
        Check.Equal(item.Status, LenientLaptopAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLaptopAgreementMovesToDone()
    {
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("code-740", "name-498");
        try
        {
            item.MoveTo(LenientLaptopAgreementStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLaptopAgreementStatus.Done, "status");
    }

    public static void LenientLaptopAgreementRejectsMovesFromFailed()
    {
        var item = LenientLaptopAgreement.NewLenientLaptopAgreement("code-740", "name-498");
        item.Status = LenientLaptopAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientLaptopAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientLaptopAgreementStartsInNew();
        LenientLaptopAgreementMovesToDone();
        LenientLaptopAgreementRejectsMovesFromFailed();
    }
}
