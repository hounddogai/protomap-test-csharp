namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position mapping is in its life cycle.</summary>
public enum PositionMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A position mapping. Its code is unique within a tenant.</summary>
public sealed class PositionMapping(string code, string title, int version, bool active, int capacity, PositionMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public int Capacity { get; set; } = capacity;
    public PositionMappingStatus Status { get; set; } = status;

    /// <summary>Starts a position mapping in status planned.</summary>
    public static PositionMapping NewPositionMapping(string code, string title)
    {
        return new PositionMapping(code, title, 0, false, 0, PositionMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionMappingStatus next)
    {
        switch (Status)
        {
            case PositionMappingStatus.Planned:
                switch (next)
                {
                    case PositionMappingStatus.Scheduled:
                        return true;
                }
                return false;
            case PositionMappingStatus.Scheduled:
                switch (next)
                {
                    case PositionMappingStatus.InProgress:
                    case PositionMappingStatus.Completed:
                        return true;
                }
                return false;
            case PositionMappingStatus.InProgress:
                return next == PositionMappingStatus.Cancelled;
            case PositionMappingStatus.Completed:
                return next == PositionMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)Capacity + (double)Version, 2);
    }

    public PositionMapping Copy()
    {
        return new PositionMapping(Code, Title, Version, Active, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
