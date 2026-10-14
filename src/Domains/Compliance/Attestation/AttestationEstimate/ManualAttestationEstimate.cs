namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual attestation estimate is in its life cycle.</summary>
public enum ManualAttestationEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a manual attestation estimate and its current status.</summary>
public sealed class ManualAttestationEstimate(string code, string label, string country, bool archived, string employeeId, double amount, ManualAttestationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public ManualAttestationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a manual attestation estimate in status planned.</summary>
    public static ManualAttestationEstimate NewManualAttestationEstimate(string code, string label)
    {
        return new ManualAttestationEstimate(
            code,
            label,
            "",
            false,
            "",
            0.0,
            ManualAttestationEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAttestationEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAttestationEstimateStatus next)
    {
        switch (Status)
        {
            case ManualAttestationEstimateStatus.Planned:
                switch (next)
                {
                    case ManualAttestationEstimateStatus.InProgress:
                    case ManualAttestationEstimateStatus.Completed:
                        return true;
                }
                return false;
            case ManualAttestationEstimateStatus.Scheduled:
                switch (next)
                {
                    case ManualAttestationEstimateStatus.InProgress:
                        return true;
                }
                return false;
            case ManualAttestationEstimateStatus.InProgress:
                return next == ManualAttestationEstimateStatus.Completed || next == ManualAttestationEstimateStatus.Cancelled;
            case ManualAttestationEstimateStatus.Completed:
                switch (next)
                {
                    case ManualAttestationEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualAttestationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual attestation estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ManualAttestationEstimate Copy()
    {
        return new ManualAttestationEstimate(Code, Label, Country, Archived, EmployeeId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
