namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path cycle is in its life cycle.</summary>
public enum CareerPathCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A career path cycle. Its code is unique within a tenant.</summary>
public sealed class CareerPathCycle(string code, string name, string checksum, string department, string contactEmail, int expiresAt, double maximum, string managerId, bool active, double rating, int attempts, CareerPathCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public string Department { get; set; } = department;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public CareerPathCycleStatus Status { get; set; } = status;

    /// <summary>Starts a career path cycle in status proposed.</summary>
    public static CareerPathCycle NewCareerPathCycle(string code, string name)
    {
        return new CareerPathCycle(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            "",
            false,
            0.0,
            0,
            CareerPathCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathCycleStatus next)
    {
        switch (Status)
        {
            case CareerPathCycleStatus.Proposed:
                switch (next)
                {
                    case CareerPathCycleStatus.Approved:
                    case CareerPathCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathCycleStatus.Approved:
                return next == CareerPathCycleStatus.Rejected;
            case CareerPathCycleStatus.Rejected:
                return next == CareerPathCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CareerPathCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetTotal()
    {
        return Math.Round(Maximum * Rating, 2);
    }

    public CareerPathCycle Copy()
    {
        return new CareerPathCycle(
            Code,
            Name,
            Checksum,
            Department,
            ContactEmail,
            ExpiresAt,
            Maximum,
            ManagerId,
            Active,
            Rating,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
