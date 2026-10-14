namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification slot is in its life cycle.</summary>
public enum CertificationSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a certification slot and its current status.</summary>
public sealed class CertificationSlot(string code, string title, string salaryBand, string employeeId, int endsAt, string checksum, string costCenter, int attempts, int tenureMonths, int revision, string jobCode, CertificationSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public string JobCode { get; set; } = jobCode;
    public CertificationSlotStatus Status { get; set; } = status;

    /// <summary>Starts a certification slot in status planned.</summary>
    public static CertificationSlot NewCertificationSlot(string code, string title)
    {
        return new CertificationSlot(code, title, "", "", 0, "", "", 0, 0, 0, "", CertificationSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationSlotStatus next)
    {
        switch (Status)
        {
            case CertificationSlotStatus.Planned:
                switch (next)
                {
                    case CertificationSlotStatus.Scheduled:
                    case CertificationSlotStatus.Completed:
                        return true;
                }
                return false;
            case CertificationSlotStatus.Scheduled:
                return next == CertificationSlotStatus.Cancelled;
            case CertificationSlotStatus.InProgress:
                return next == CertificationSlotStatus.Completed || next == CertificationSlotStatus.Cancelled;
            case CertificationSlotStatus.Completed:
                return next == CertificationSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CertificationSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"certification slot"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts + (double)TenureMonths, 2);
    }

    public CertificationSlot Copy()
    {
        return new CertificationSlot(
            Code,
            Title,
            SalaryBand,
            EmployeeId,
            EndsAt,
            Checksum,
            CostCenter,
            Attempts,
            TenureMonths,
            Revision,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
