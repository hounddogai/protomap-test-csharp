namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation adjustment is in its life cycle.</summary>
public enum AttestationAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One attestation adjustment, with what the planners need to act on it.</summary>
public sealed class AttestationAdjustment(string code, string title, string externalRef, string category, double ratio, AttestationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Category { get; set; } = category;
    public double Ratio { get; set; } = ratio;
    public AttestationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a attestation adjustment in status draft.</summary>
    public static AttestationAdjustment NewAttestationAdjustment(string code, string title)
    {
        return new AttestationAdjustment(code, title, "", "", 0.0, AttestationAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationAdjustmentStatus next)
    {
        switch (Status)
        {
            case AttestationAdjustmentStatus.Draft:
                return next == AttestationAdjustmentStatus.Closed;
            case AttestationAdjustmentStatus.Pending:
                return next == AttestationAdjustmentStatus.Suspended || next == AttestationAdjustmentStatus.Closed;
            case AttestationAdjustmentStatus.Active:
                switch (next)
                {
                    case AttestationAdjustmentStatus.Suspended:
                    case AttestationAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case AttestationAdjustmentStatus.Suspended:
                return next == AttestationAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AttestationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public AttestationAdjustment Copy()
    {
        return new AttestationAdjustment(Code, Title, ExternalRef, Category, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
