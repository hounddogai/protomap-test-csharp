namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall allocation is in its life cycle.</summary>
public enum TownHallAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A town hall allocation. Its code is unique within a tenant.</summary>
public sealed class TownHallAllocation(string code, string name, Dictionary<string, string> attributes, int limit, double rating, string grade, TownHallAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public string Grade { get; set; } = grade;
    public TownHallAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a town hall allocation in status planned.</summary>
    public static TownHallAllocation NewTownHallAllocation(string code, string name)
    {
        return new TownHallAllocation(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0.0,
            "",
            TownHallAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallAllocationStatus next)
    {
        switch (Status)
        {
            case TownHallAllocationStatus.Planned:
                switch (next)
                {
                    case TownHallAllocationStatus.Scheduled:
                    case TownHallAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallAllocationStatus.Scheduled:
                switch (next)
                {
                    case TownHallAllocationStatus.Completed:
                    case TownHallAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallAllocationStatus.InProgress:
                return next == TownHallAllocationStatus.Completed || next == TownHallAllocationStatus.Cancelled;
            case TownHallAllocationStatus.Completed:
                switch (next)
                {
                    case TownHallAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetExposure()
    {
        return Math.Round((double)Limit - Rating, 2);
    }

    public TownHallAllocation Copy()
    {
        return new TownHallAllocation(Code, Name, Attributes, Limit, Rating, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
