namespace Acme.Workforce.Contractors.Tests.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class VendorAgreementChecks
{
    public static void VendorAgreementStartsInPlanned()
    {
        var item = VendorAgreement.NewVendorAgreement("code-153", "name-931");
        Check.Equal(item.Status, VendorAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorAgreementMovesToInProgress()
    {
        var item = VendorAgreement.NewVendorAgreement("code-153", "name-931");
        try
        {
            item.MoveTo(VendorAgreementStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorAgreementStatus.InProgress, "status");
    }

    public static void VendorAgreementRejectsMovesFromCancelled()
    {
        var item = VendorAgreement.NewVendorAgreement("code-153", "name-931");
        item.Status = VendorAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(VendorAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        VendorAgreementStartsInPlanned();
        VendorAgreementMovesToInProgress();
        VendorAgreementRejectsMovesFromCancelled();
    }
}
