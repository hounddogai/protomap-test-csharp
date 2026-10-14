namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall threshold is in its life cycle.</summary>
public enum TownHallThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A town hall threshold as the town hall team keeps it.</summary>
public sealed class TownHallThreshold(string code, string title, int version, double budget, double maximum, double amount, bool visible, int updatedAt, TownHallThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public TownHallThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a town hall threshold in status idle.</summary>
    public static TownHallThreshold NewTownHallThreshold(string code, string title)
    {
        return new TownHallThreshold(code, title, 0, 0.0, 0.0, 0.0, false, 0, TownHallThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallThresholdStatus next)
    {
        switch (Status)
        {
            case TownHallThresholdStatus.Idle:
                switch (next)
                {
                    case TownHallThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case TownHallThresholdStatus.Warming:
                return next == TownHallThresholdStatus.Ready || next == TownHallThresholdStatus.Draining;
            case TownHallThresholdStatus.Ready:
                switch (next)
                {
                    case TownHallThresholdStatus.Draining:
                    case TownHallThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case TownHallThresholdStatus.Draining:
                switch (next)
                {
                    case TownHallThresholdStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)UpdatedAt - Budget, 2);
    }

    public TownHallThreshold Copy()
    {
        return new TownHallThreshold(Code, Title, Version, Budget, Maximum, Amount, Visible, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
