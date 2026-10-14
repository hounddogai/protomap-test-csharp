namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay threshold is in its life cycle.</summary>
public enum RetroPayThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One retro pay threshold, with what the planners need to act on it.</summary>
public sealed class RetroPayThreshold(string code, string label, double ceiling, string location, int updatedAt, bool archived, double cost, RetroPayThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public double Cost { get; set; } = cost;
    public RetroPayThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay threshold in status planned.</summary>
    public static RetroPayThreshold NewRetroPayThreshold(string code, string label)
    {
        return new RetroPayThreshold(code, label, 0.0, "", 0, false, 0.0, RetroPayThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayThresholdStatus next)
    {
        switch (Status)
        {
            case RetroPayThresholdStatus.Planned:
                switch (next)
                {
                    case RetroPayThresholdStatus.InProgress:
                    case RetroPayThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayThresholdStatus.Scheduled:
                return next == RetroPayThresholdStatus.Completed;
            case RetroPayThresholdStatus.InProgress:
                return next == RetroPayThresholdStatus.Completed || next == RetroPayThresholdStatus.Cancelled;
            case RetroPayThresholdStatus.Completed:
                switch (next)
                {
                    case RetroPayThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling + Cost, 2);
    }

    public RetroPayThreshold Copy()
    {
        return new RetroPayThreshold(Code, Label, Ceiling, Location, UpdatedAt, Archived, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
