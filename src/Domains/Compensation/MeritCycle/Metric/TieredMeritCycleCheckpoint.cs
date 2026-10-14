namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered merit cycle checkpoint is in its life cycle.</summary>
public enum TieredMeritCycleCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered merit cycle checkpoint. Its code is unique within a tenant.</summary>
public sealed class TieredMeritCycleCheckpoint(string code, string title, double amount, double ceiling, int expiresAt, TieredMeritCycleCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public TieredMeritCycleCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a tiered merit cycle checkpoint in status requested.</summary>
    public static TieredMeritCycleCheckpoint NewTieredMeritCycleCheckpoint(string code, string title)
    {
        return new TieredMeritCycleCheckpoint(code, title, 0.0, 0.0, 0, TieredMeritCycleCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredMeritCycleCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredMeritCycleCheckpointStatus next)
    {
        switch (Status)
        {
            case TieredMeritCycleCheckpointStatus.Requested:
                return next == TieredMeritCycleCheckpointStatus.Granted || next == TieredMeritCycleCheckpointStatus.Expired;
            case TieredMeritCycleCheckpointStatus.Granted:
                return next == TieredMeritCycleCheckpointStatus.Revoked || next == TieredMeritCycleCheckpointStatus.Expired;
            case TieredMeritCycleCheckpointStatus.Revoked:
                switch (next)
                {
                    case TieredMeritCycleCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredMeritCycleCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered merit cycle checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt + Amount, 2);
    }

    public TieredMeritCycleCheckpoint Copy()
    {
        return new TieredMeritCycleCheckpoint(Code, Title, Amount, Ceiling, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
