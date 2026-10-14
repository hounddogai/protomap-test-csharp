namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool adjustment is in its life cycle.</summary>
public enum PoolAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pool adjustment. Its code is unique within a tenant.</summary>
public sealed class PoolAdjustment(string code, string name, string source, string skill, int endsAt, string department, int leaveDays, double threshold, string phone, int revision, string reviewer, PoolAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public int LeaveDays { get; set; } = leaveDays;
    public double Threshold { get; set; } = threshold;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public string Reviewer { get; set; } = reviewer;
    public PoolAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pool adjustment in status requested.</summary>
    public static PoolAdjustment NewPoolAdjustment(string code, string name)
    {
        return new PoolAdjustment(code, name, "", "", 0, "", 0, 0.0, "", 0, "", PoolAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolAdjustmentStatus next)
    {
        switch (Status)
        {
            case PoolAdjustmentStatus.Requested:
                switch (next)
                {
                    case PoolAdjustmentStatus.Revoked:
                        return true;
                }
                return false;
            case PoolAdjustmentStatus.Granted:
                switch (next)
                {
                    case PoolAdjustmentStatus.Revoked:
                        return true;
                }
                return false;
            case PoolAdjustmentStatus.Revoked:
                switch (next)
                {
                    case PoolAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)EndsAt - Threshold, 2);
    }

    public PoolAdjustment Copy()
    {
        return new PoolAdjustment(
            Code,
            Name,
            Source,
            Skill,
            EndsAt,
            Department,
            LeaveDays,
            Threshold,
            Phone,
            Revision,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
