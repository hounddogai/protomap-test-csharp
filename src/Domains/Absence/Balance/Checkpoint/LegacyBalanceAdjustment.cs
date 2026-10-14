namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy balance adjustment is in its life cycle.</summary>
public enum LegacyBalanceAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy balance adjustment as the balance team keeps it.</summary>
public sealed class LegacyBalanceAdjustment(string code, string title, int attempts, int durationMinutes, double maximum, string team, string managerId, int expiresAt, string channel, double weight, LegacyBalanceAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Maximum { get; set; } = maximum;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public LegacyBalanceAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy balance adjustment in status draft.</summary>
    public static LegacyBalanceAdjustment NewLegacyBalanceAdjustment(string code, string title)
    {
        return new LegacyBalanceAdjustment(
            code,
            title,
            0,
            0,
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            LegacyBalanceAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyBalanceAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyBalanceAdjustmentStatus next)
    {
        switch (Status)
        {
            case LegacyBalanceAdjustmentStatus.Draft:
                return next == LegacyBalanceAdjustmentStatus.Pending;
            case LegacyBalanceAdjustmentStatus.Pending:
                switch (next)
                {
                    case LegacyBalanceAdjustmentStatus.Active:
                        return true;
                }
                return false;
            case LegacyBalanceAdjustmentStatus.Active:
                switch (next)
                {
                    case LegacyBalanceAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case LegacyBalanceAdjustmentStatus.Suspended:
                switch (next)
                {
                    case LegacyBalanceAdjustmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyBalanceAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy balance adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)DurationMinutes * (double)Attempts, 2);
    }

    public LegacyBalanceAdjustment Copy()
    {
        return new LegacyBalanceAdjustment(
            Code,
            Title,
            Attempts,
            DurationMinutes,
            Maximum,
            Team,
            ManagerId,
            ExpiresAt,
            Channel,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
