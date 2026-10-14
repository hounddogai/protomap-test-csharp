namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation draft is in its life cycle.</summary>
public enum AttestationDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a attestation draft and its current status.</summary>
public sealed class AttestationDraft(string code, string label, double cost, double rating, string reviewer, int expiresAt, int durationMinutes, int sequence, string channel, int attempts, AttestationDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public double Rating { get; set; } = rating;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public int Attempts { get; set; } = attempts;
    public AttestationDraftStatus Status { get; set; } = status;

    /// <summary>Starts a attestation draft in status new.</summary>
    public static AttestationDraft NewAttestationDraft(string code, string label)
    {
        return new AttestationDraft(code, label, 0.0, 0.0, "", 0, 0, 0, "", 0, AttestationDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationDraftStatus next)
    {
        switch (Status)
        {
            case AttestationDraftStatus.New:
                switch (next)
                {
                    case AttestationDraftStatus.Failed:
                        return true;
                }
                return false;
            case AttestationDraftStatus.Queued:
                switch (next)
                {
                    case AttestationDraftStatus.Failed:
                        return true;
                }
                return false;
            case AttestationDraftStatus.Running:
                switch (next)
                {
                    case AttestationDraftStatus.Done:
                    case AttestationDraftStatus.Failed:
                        return true;
                }
                return false;
            case AttestationDraftStatus.Done:
                switch (next)
                {
                    case AttestationDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Sequence + Cost, 2);
    }

    public AttestationDraft Copy()
    {
        return new AttestationDraft(
            Code,
            Label,
            Cost,
            Rating,
            Reviewer,
            ExpiresAt,
            DurationMinutes,
            Sequence,
            Channel,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
