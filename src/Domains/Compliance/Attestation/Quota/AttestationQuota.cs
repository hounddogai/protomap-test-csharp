namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation quota is in its life cycle.</summary>
public enum AttestationQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One attestation quota, with what the planners need to act on it.</summary>
public sealed class AttestationQuota(string code, string title, string name, string source, double minimum, int retries, int sequence, AttestationQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public int Sequence { get; set; } = sequence;
    public AttestationQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a attestation quota in status planned.</summary>
    public static AttestationQuota NewAttestationQuota(string code, string title)
    {
        return new AttestationQuota(code, title, "", "", 0.0, 0, 0, AttestationQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationQuotaStatus next)
    {
        switch (Status)
        {
            case AttestationQuotaStatus.Planned:
                switch (next)
                {
                    case AttestationQuotaStatus.InProgress:
                    case AttestationQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case AttestationQuotaStatus.Scheduled:
                return next == AttestationQuotaStatus.InProgress || next == AttestationQuotaStatus.Cancelled;
            case AttestationQuotaStatus.InProgress:
                switch (next)
                {
                    case AttestationQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case AttestationQuotaStatus.Completed:
                switch (next)
                {
                    case AttestationQuotaStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation quota"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Sequence - Minimum, 2);
    }

    public AttestationQuota Copy()
    {
        return new AttestationQuota(Code, Title, Name, Source, Minimum, Retries, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
