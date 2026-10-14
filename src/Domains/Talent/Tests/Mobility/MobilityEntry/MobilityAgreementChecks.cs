namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityAgreementChecks
{
    public static void MobilityAgreementStartsInNew()
    {
        var item = MobilityAgreement.NewMobilityAgreement("code-898", "label-509");
        Check.Equal(item.Status, MobilityAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityAgreementMovesToQueued()
    {
        var item = MobilityAgreement.NewMobilityAgreement("code-898", "label-509");
        try
        {
            item.MoveTo(MobilityAgreementStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityAgreementStatus.Queued, "status");
    }

    public static void MobilityAgreementRejectsMovesFromFailed()
    {
        var item = MobilityAgreement.NewMobilityAgreement("code-898", "label-509");
        item.Status = MobilityAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityAgreementStartsInNew();
        MobilityAgreementMovesToQueued();
        MobilityAgreementRejectsMovesFromFailed();
    }
}
