namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback attestation summary is in its life cycle.</summary>
public enum FallbackAttestationSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback attestation summary, with what the planners need to act on it.</summary>
public sealed class FallbackAttestationSummary(string code, string name, string checksum, string source, string team, int quantity, FallbackAttestationSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Source { get; set; } = source;
    public string Team { get; set; } = team;
    public int Quantity { get; set; } = quantity;
    public FallbackAttestationSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a fallback attestation summary in status requested.</summary>
    public static FallbackAttestationSummary NewFallbackAttestationSummary(string code, string name)
    {
        return new FallbackAttestationSummary(code, name, "", "", "", 0, FallbackAttestationSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAttestationSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAttestationSummaryStatus next)
    {
        switch (Status)
        {
            case FallbackAttestationSummaryStatus.Requested:
                return next == FallbackAttestationSummaryStatus.Granted;
            case FallbackAttestationSummaryStatus.Granted:
                switch (next)
                {
                    case FallbackAttestationSummaryStatus.Expired:
                        return true;
                }
                return false;
            case FallbackAttestationSummaryStatus.Revoked:
                switch (next)
                {
                    case FallbackAttestationSummaryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAttestationSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback attestation summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public FallbackAttestationSummary Copy()
    {
        return new FallbackAttestationSummary(Code, Name, Checksum, Source, Team, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
