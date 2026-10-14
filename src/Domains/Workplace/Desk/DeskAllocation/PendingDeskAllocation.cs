namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending desk allocation is in its life cycle.</summary>
public enum PendingDeskAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending desk allocation, with what the planners need to act on it.</summary>
public sealed class PendingDeskAllocation(string code, string label, string team, string badge, int endsAt, string channel, string location, int retries, string costCenter, PendingDeskAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public int Retries { get; set; } = retries;
    public string CostCenter { get; set; } = costCenter;
    public PendingDeskAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending desk allocation in status planned.</summary>
    public static PendingDeskAllocation NewPendingDeskAllocation(string code, string label)
    {
        return new PendingDeskAllocation(code, label, "", "", 0, "", "", 0, "", PendingDeskAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeskAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeskAllocationStatus next)
    {
        switch (Status)
        {
            case PendingDeskAllocationStatus.Planned:
                return next == PendingDeskAllocationStatus.Scheduled || next == PendingDeskAllocationStatus.InProgress;
            case PendingDeskAllocationStatus.Scheduled:
                return next == PendingDeskAllocationStatus.InProgress;
            case PendingDeskAllocationStatus.InProgress:
                switch (next)
                {
                    case PendingDeskAllocationStatus.Completed:
                    case PendingDeskAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingDeskAllocationStatus.Completed:
                switch (next)
                {
                    case PendingDeskAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingDeskAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending desk allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Retries - (double)EndsAt, 2);
    }

    public PendingDeskAllocation Copy()
    {
        return new PendingDeskAllocation(
            Code,
            Label,
            Team,
            Badge,
            EndsAt,
            Channel,
            Location,
            Retries,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
