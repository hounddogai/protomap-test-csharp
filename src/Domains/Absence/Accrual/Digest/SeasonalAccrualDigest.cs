namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal accrual digest is in its life cycle.</summary>
public enum SeasonalAccrualDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal accrual digest and its current status.</summary>
public sealed class SeasonalAccrualDigest(string code, string name, string managerId, string employeeId, string checksum, double budget, double hours, bool archived, SeasonalAccrualDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public string EmployeeId { get; set; } = employeeId;
    public string Checksum { get; set; } = checksum;
    public double Budget { get; set; } = budget;
    public double Hours { get; set; } = hours;
    public bool Archived { get; set; } = archived;
    public SeasonalAccrualDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal accrual digest in status proposed.</summary>
    public static SeasonalAccrualDigest NewSeasonalAccrualDigest(string code, string name)
    {
        return new SeasonalAccrualDigest(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            false,
            SeasonalAccrualDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccrualDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccrualDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalAccrualDigestStatus.Proposed:
                return next == SeasonalAccrualDigestStatus.Approved;
            case SeasonalAccrualDigestStatus.Approved:
                return next == SeasonalAccrualDigestStatus.Withdrawn;
            case SeasonalAccrualDigestStatus.Rejected:
                return next == SeasonalAccrualDigestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalAccrualDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal accrual digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Budget - Hours, 2);
    }

    public SeasonalAccrualDigest Copy()
    {
        return new SeasonalAccrualDigest(
            Code,
            Name,
            ManagerId,
            EmployeeId,
            Checksum,
            Budget,
            Hours,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
