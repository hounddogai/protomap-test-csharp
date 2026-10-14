namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation record is in its life cycle.</summary>
public enum AttestationRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A attestation record as the attestation team keeps it.</summary>
public sealed class AttestationRecord(string code, string name, string checksum, int graceDays, double budget, bool active, double threshold, AttestationRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public bool Active { get; set; } = active;
    public double Threshold { get; set; } = threshold;
    public AttestationRecordStatus Status { get; set; } = status;

    /// <summary>Starts a attestation record in status open.</summary>
    public static AttestationRecord NewAttestationRecord(string code, string name)
    {
        return new AttestationRecord(code, name, "", 0, 0.0, false, 0.0, AttestationRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationRecordStatus next)
    {
        switch (Status)
        {
            case AttestationRecordStatus.Open:
                switch (next)
                {
                    case AttestationRecordStatus.Archived:
                        return true;
                }
                return false;
            case AttestationRecordStatus.InReview:
                switch (next)
                {
                    case AttestationRecordStatus.Reopened:
                    case AttestationRecordStatus.Archived:
                        return true;
                }
                return false;
            case AttestationRecordStatus.Resolved:
                return next == AttestationRecordStatus.Reopened;
            case AttestationRecordStatus.Reopened:
                return next == AttestationRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AttestationRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Threshold + Budget, 2);
    }

    public AttestationRecord Copy()
    {
        return new AttestationRecord(Code, Name, Checksum, GraceDays, Budget, Active, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
