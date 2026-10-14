namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal assignment allocation is in its life cycle.</summary>
public enum SeasonalAssignmentAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One seasonal assignment allocation, with what the planners need to act on it.</summary>
public sealed class SeasonalAssignmentAllocation(string code, string name, double hours, string currency, bool active, double margin, SeasonalAssignmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public bool Active { get; set; } = active;
    public double Margin { get; set; } = margin;
    public SeasonalAssignmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal assignment allocation in status draft.</summary>
    public static SeasonalAssignmentAllocation NewSeasonalAssignmentAllocation(string code, string name)
    {
        return new SeasonalAssignmentAllocation(
            code,
            name,
            0.0,
            "",
            false,
            0.0,
            SeasonalAssignmentAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAssignmentAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAssignmentAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalAssignmentAllocationStatus.Draft:
                switch (next)
                {
                    case SeasonalAssignmentAllocationStatus.Pending:
                        return true;
                }
                return false;
            case SeasonalAssignmentAllocationStatus.Pending:
                switch (next)
                {
                    case SeasonalAssignmentAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalAssignmentAllocationStatus.Active:
                switch (next)
                {
                    case SeasonalAssignmentAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalAssignmentAllocationStatus.Suspended:
                return next == SeasonalAssignmentAllocationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalAssignmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal assignment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin * Hours, 2);
    }

    public SeasonalAssignmentAllocation Copy()
    {
        return new SeasonalAssignmentAllocation(Code, Name, Hours, Currency, Active, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
