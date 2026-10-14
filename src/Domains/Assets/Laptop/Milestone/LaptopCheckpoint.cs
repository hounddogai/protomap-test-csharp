namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop checkpoint is in its life cycle.</summary>
public enum LaptopCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A laptop checkpoint as the laptop team keeps it.</summary>
public sealed class LaptopCheckpoint(string code, string label, double hours, double threshold, int expiresAt, string costCenter, LaptopCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Threshold { get; set; } = threshold;
    public int ExpiresAt { get; set; } = expiresAt;
    public string CostCenter { get; set; } = costCenter;
    public LaptopCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a laptop checkpoint in status planned.</summary>
    public static LaptopCheckpoint NewLaptopCheckpoint(string code, string label)
    {
        return new LaptopCheckpoint(code, label, 0.0, 0.0, 0, "", LaptopCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopCheckpointStatus next)
    {
        switch (Status)
        {
            case LaptopCheckpointStatus.Planned:
                switch (next)
                {
                    case LaptopCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopCheckpointStatus.Scheduled:
                switch (next)
                {
                    case LaptopCheckpointStatus.Completed:
                    case LaptopCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopCheckpointStatus.InProgress:
                return next == LaptopCheckpointStatus.Completed || next == LaptopCheckpointStatus.Cancelled;
            case LaptopCheckpointStatus.Completed:
                return next == LaptopCheckpointStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LaptopCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"laptop checkpoint"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Hours - Threshold, 2);
    }

    public LaptopCheckpoint Copy()
    {
        return new LaptopCheckpoint(Code, Label, Hours, Threshold, ExpiresAt, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
