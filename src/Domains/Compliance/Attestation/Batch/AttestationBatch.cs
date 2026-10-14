namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation batch is in its life cycle.</summary>
public enum AttestationBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a attestation batch and its current status.</summary>
public sealed class AttestationBatch(string code, string name, string externalRef, double hours, string locale, string segment, string source, int tenureMonths, string region, string employeeId, AttestationBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Hours { get; set; } = hours;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public AttestationBatchStatus Status { get; set; } = status;

    /// <summary>Starts a attestation batch in status new.</summary>
    public static AttestationBatch NewAttestationBatch(string code, string name)
    {
        return new AttestationBatch(code, name, "", 0.0, "", "", "", 0, "", "", AttestationBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationBatchStatus next)
    {
        switch (Status)
        {
            case AttestationBatchStatus.New:
                return next == AttestationBatchStatus.Done;
            case AttestationBatchStatus.Queued:
                switch (next)
                {
                    case AttestationBatchStatus.Running:
                    case AttestationBatchStatus.Failed:
                        return true;
                }
                return false;
            case AttestationBatchStatus.Running:
                switch (next)
                {
                    case AttestationBatchStatus.Done:
                    case AttestationBatchStatus.Failed:
                        return true;
                }
                return false;
            case AttestationBatchStatus.Done:
                return next == AttestationBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AttestationBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Hours * (double)TenureMonths, 2);
    }

    public AttestationBatch Copy()
    {
        return new AttestationBatch(
            Code,
            Name,
            ExternalRef,
            Hours,
            Locale,
            Segment,
            Source,
            TenureMonths,
            Region,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
