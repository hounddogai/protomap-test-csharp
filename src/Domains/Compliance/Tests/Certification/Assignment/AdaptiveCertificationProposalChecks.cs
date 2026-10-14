namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AdaptiveCertificationProposalChecks
{
    public static void AdaptiveCertificationProposalStartsInCreated()
    {
        var item = AdaptiveCertificationProposal.NewAdaptiveCertificationProposal("code-290", "label-777");
        Check.Equal(item.Status, AdaptiveCertificationProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCertificationProposalMovesToValidated()
    {
        var item = AdaptiveCertificationProposal.NewAdaptiveCertificationProposal("code-290", "label-777");
        try
        {
            item.MoveTo(AdaptiveCertificationProposalStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCertificationProposalStatus.Validated, "status");
    }

    public static void AdaptiveCertificationProposalRejectsMovesFromRetired()
    {
        var item = AdaptiveCertificationProposal.NewAdaptiveCertificationProposal("code-290", "label-777");
        item.Status = AdaptiveCertificationProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveCertificationProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveCertificationProposalStartsInCreated();
        AdaptiveCertificationProposalMovesToValidated();
        AdaptiveCertificationProposalRejectsMovesFromRetired();
    }
}
