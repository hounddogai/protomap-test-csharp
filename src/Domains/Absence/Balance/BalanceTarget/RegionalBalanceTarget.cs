namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional balance target is in its life cycle.</summary>
public enum RegionalBalanceTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional balance target. Its code is unique within a tenant.</summary>
public sealed class RegionalBalanceTarget(string code, string name, int durationMinutes, bool pinned, string phone, bool active, string unit, double budget, RegionalBalanceTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public string Unit { get; set; } = unit;
    public double Budget { get; set; } = budget;
    public RegionalBalanceTargetStatus Status { get; set; } = status;

    /// <summary>Starts a regional balance target in status proposed.</summary>
    public static RegionalBalanceTarget NewRegionalBalanceTarget(string code, string name)
    {
        return new RegionalBalanceTarget(
            code,
            name,
            0,
            false,
            "",
            false,
            "",
            0.0,
            RegionalBalanceTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBalanceTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBalanceTargetStatus next)
    {
        switch (Status)
        {
            case RegionalBalanceTargetStatus.Proposed:
                return next == RegionalBalanceTargetStatus.Rejected;
            case RegionalBalanceTargetStatus.Approved:
                switch (next)
                {
                    case RegionalBalanceTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalBalanceTargetStatus.Rejected:
                return next == RegionalBalanceTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalBalanceTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional balance target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes * Budget, 2);
    }

    public RegionalBalanceTarget Copy()
    {
        return new RegionalBalanceTarget(Code, Name, DurationMinutes, Pinned, Phone, Active, Unit, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
