namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation exception is in its life cycle.</summary>
public enum AttestationExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a attestation exception and its current status.</summary>
public sealed class AttestationException(string code, string title, int priority, double margin, bool visible, double hours, double floorValue, int endsAt, string checksum, string locale, string badge, AttestationExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double Margin { get; set; } = margin;
    public bool Visible { get; set; } = visible;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public string Locale { get; set; } = locale;
    public string Badge { get; set; } = badge;
    public AttestationExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a attestation exception in status planned.</summary>
    public static AttestationException NewAttestationException(string code, string title)
    {
        return new AttestationException(
            code,
            title,
            0,
            0.0,
            false,
            0.0,
            0.0,
            0,
            "",
            "",
            "",
            AttestationExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationExceptionStatus next)
    {
        switch (Status)
        {
            case AttestationExceptionStatus.Planned:
                switch (next)
                {
                    case AttestationExceptionStatus.Scheduled:
                        return true;
                }
                return false;
            case AttestationExceptionStatus.Scheduled:
                return next == AttestationExceptionStatus.InProgress || next == AttestationExceptionStatus.Cancelled;
            case AttestationExceptionStatus.InProgress:
                return next == AttestationExceptionStatus.Completed;
            case AttestationExceptionStatus.Completed:
                return next == AttestationExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AttestationExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt * FloorValue, 2);
    }

    public AttestationException Copy()
    {
        return new AttestationException(
            Code,
            Title,
            Priority,
            Margin,
            Visible,
            Hours,
            FloorValue,
            EndsAt,
            Checksum,
            Locale,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
