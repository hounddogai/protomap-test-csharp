namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty checkpoint is in its life cycle.</summary>
public enum WarrantyCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a warranty checkpoint and its current status.</summary>
public sealed class WarrantyCheckpoint(string code, string title, string badge, int limit, string timezone, string name, double weight, int windowDays, WarrantyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int WindowDays { get; set; } = windowDays;
    public WarrantyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a warranty checkpoint in status planned.</summary>
    public static WarrantyCheckpoint NewWarrantyCheckpoint(string code, string title)
    {
        return new WarrantyCheckpoint(code, title, "", 0, "", "", 0.0, 0, WarrantyCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyCheckpointStatus next)
    {
        switch (Status)
        {
            case WarrantyCheckpointStatus.Planned:
                switch (next)
                {
                    case WarrantyCheckpointStatus.Scheduled:
                    case WarrantyCheckpointStatus.Completed:
                        return true;
                }
                return false;
            case WarrantyCheckpointStatus.Scheduled:
                return next == WarrantyCheckpointStatus.InProgress;
            case WarrantyCheckpointStatus.InProgress:
                switch (next)
                {
                    case WarrantyCheckpointStatus.Completed:
                    case WarrantyCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case WarrantyCheckpointStatus.Completed:
                switch (next)
                {
                    case WarrantyCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)WindowDays - Weight, 2);
    }

    public WarrantyCheckpoint Copy()
    {
        return new WarrantyCheckpoint(Code, Title, Badge, Limit, Timezone, Name, Weight, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
