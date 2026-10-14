namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary bonus pool group is in its life cycle.</summary>
public enum PrimaryBonusPoolGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A primary bonus pool group. Its code is unique within a tenant.</summary>
public sealed class PrimaryBonusPoolGroup(string code, string name, int sequence, int endsAt, int version, string employeeId, int batchSize, string skill, int windowDays, string segment, PrimaryBonusPoolGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public string Skill { get; set; } = skill;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public PrimaryBonusPoolGroupStatus Status { get; set; } = status;

    /// <summary>Starts a primary bonus pool group in status new.</summary>
    public static PrimaryBonusPoolGroup NewPrimaryBonusPoolGroup(string code, string name)
    {
        return new PrimaryBonusPoolGroup(code, name, 0, 0, 0, "", 0, "", 0, "", PrimaryBonusPoolGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBonusPoolGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBonusPoolGroupStatus next)
    {
        switch (Status)
        {
            case PrimaryBonusPoolGroupStatus.New:
                switch (next)
                {
                    case PrimaryBonusPoolGroupStatus.Done:
                    case PrimaryBonusPoolGroupStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryBonusPoolGroupStatus.Queued:
                switch (next)
                {
                    case PrimaryBonusPoolGroupStatus.Done:
                    case PrimaryBonusPoolGroupStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryBonusPoolGroupStatus.Running:
                switch (next)
                {
                    case PrimaryBonusPoolGroupStatus.Done:
                    case PrimaryBonusPoolGroupStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryBonusPoolGroupStatus.Done:
                return next == PrimaryBonusPoolGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryBonusPoolGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary bonus pool group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Version + (double)EndsAt, 2);
    }

    public PrimaryBonusPoolGroup Copy()
    {
        return new PrimaryBonusPoolGroup(
            Code,
            Name,
            Sequence,
            EndsAt,
            Version,
            EmployeeId,
            BatchSize,
            Skill,
            WindowDays,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
