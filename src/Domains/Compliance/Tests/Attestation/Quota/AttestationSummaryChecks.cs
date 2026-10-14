namespace Acme.Workforce.Compliance.Tests.Attestation.Quota;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AttestationSummaryChecks
{
    public static void AttestationSummaryStartsInCreated()
    {
        var item = AttestationSummary.NewAttestationSummary("code-83", "label-679");
        Check.Equal(item.Status, AttestationSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationSummaryMovesToPublished()
    {
        var item = AttestationSummary.NewAttestationSummary("code-83", "label-679");
        try
        {
            item.MoveTo(AttestationSummaryStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationSummaryStatus.Published, "status");
    }

    public static void AttestationSummaryRejectsMovesFromRetired()
    {
        var item = AttestationSummary.NewAttestationSummary("code-83", "label-679");
        item.Status = AttestationSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(AttestationSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        AttestationSummaryStartsInCreated();
        AttestationSummaryMovesToPublished();
        AttestationSummaryRejectsMovesFromRetired();
    }
}
