namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling mobility allocation is in its life cycle.</summary>
public enum RollingMobilityAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling mobility allocation as the mobility team keeps it.</summary>
public sealed class RollingMobilityAllocation(string code, string name, string reviewer, int updatedAt, double minimum, string grade, int startsAt, int sequence, string employeeId, int graceDays, bool visible, RollingMobilityAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public int Sequence { get; set; } = sequence;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public bool Visible { get; set; } = visible;
    public RollingMobilityAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling mobility allocation in status created.</summary>
    public static RollingMobilityAllocation NewRollingMobilityAllocation(string code, string name)
    {
        return new RollingMobilityAllocation(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0,
            0,
            "",
            0,
            false,
            RollingMobilityAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMobilityAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMobilityAllocationStatus next)
    {
        switch (Status)
        {
            case RollingMobilityAllocationStatus.Created:
                return next == RollingMobilityAllocationStatus.Validated || next == RollingMobilityAllocationStatus.Retired;
            case RollingMobilityAllocationStatus.Validated:
                return next == RollingMobilityAllocationStatus.Published || next == RollingMobilityAllocationStatus.Retired;
            case RollingMobilityAllocationStatus.Published:
                switch (next)
                {
                    case RollingMobilityAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingMobilityAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling mobility allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling mobility allocation"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Sequence * (double)UpdatedAt, 2);
    }

    public RollingMobilityAllocation Copy()
    {
        return new RollingMobilityAllocation(
            Code,
            Name,
            Reviewer,
            UpdatedAt,
            Minimum,
            Grade,
            StartsAt,
            Sequence,
            EmployeeId,
            GraceDays,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
