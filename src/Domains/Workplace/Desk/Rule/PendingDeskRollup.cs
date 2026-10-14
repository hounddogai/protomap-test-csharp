namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending desk rollup is in its life cycle.</summary>
public enum PendingDeskRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending desk rollup as the desk team keeps it.</summary>
public sealed class PendingDeskRollup(string code, string name, int expiresAt, string checksum, string costCenter, double ceiling, string badge, List<string> tags, bool active, PendingDeskRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public string CostCenter { get; set; } = costCenter;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public bool Active { get; set; } = active;
    public PendingDeskRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pending desk rollup in status new.</summary>
    public static PendingDeskRollup NewPendingDeskRollup(string code, string name)
    {
        return new PendingDeskRollup(
            code,
            name,
            0,
            "",
            "",
            0.0,
            "",
            new List<string>(),
            false,
            PendingDeskRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeskRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeskRollupStatus next)
    {
        switch (Status)
        {
            case PendingDeskRollupStatus.New:
                return next == PendingDeskRollupStatus.Queued || next == PendingDeskRollupStatus.Running;
            case PendingDeskRollupStatus.Queued:
                return next == PendingDeskRollupStatus.Failed;
            case PendingDeskRollupStatus.Running:
                switch (next)
                {
                    case PendingDeskRollupStatus.Done:
                    case PendingDeskRollupStatus.Failed:
                        return true;
                }
                return false;
            case PendingDeskRollupStatus.Done:
                return next == PendingDeskRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingDeskRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending desk rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling + (double)ExpiresAt, 2);
    }

    public PendingDeskRollup Copy()
    {
        return new PendingDeskRollup(
            Code,
            Name,
            ExpiresAt,
            Checksum,
            CostCenter,
            Ceiling,
            Badge,
            Tags,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
