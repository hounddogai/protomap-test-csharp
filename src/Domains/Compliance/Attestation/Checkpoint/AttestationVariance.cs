namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation variance is in its life cycle.</summary>
public enum AttestationVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One attestation variance, with what the planners need to act on it.</summary>
public sealed class AttestationVariance(string code, string title, string contactEmail, string region, double cost, double weight, bool active, string phone, string ownerId, AttestationVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public double Cost { get; set; } = cost;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public string Phone { get; set; } = phone;
    public string OwnerId { get; set; } = ownerId;
    public AttestationVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a attestation variance in status new.</summary>
    public static AttestationVariance NewAttestationVariance(string code, string title)
    {
        return new AttestationVariance(code, title, "", "", 0.0, 0.0, false, "", "", AttestationVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationVarianceStatus next)
    {
        switch (Status)
        {
            case AttestationVarianceStatus.New:
                return next == AttestationVarianceStatus.Done || next == AttestationVarianceStatus.Failed;
            case AttestationVarianceStatus.Queued:
                switch (next)
                {
                    case AttestationVarianceStatus.Running:
                        return true;
                }
                return false;
            case AttestationVarianceStatus.Running:
                return next == AttestationVarianceStatus.Done || next == AttestationVarianceStatus.Failed;
            case AttestationVarianceStatus.Done:
                return next == AttestationVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AttestationVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Weight + Cost, 2);
    }

    public AttestationVariance Copy()
    {
        return new AttestationVariance(
            Code,
            Title,
            ContactEmail,
            Region,
            Cost,
            Weight,
            Active,
            Phone,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
