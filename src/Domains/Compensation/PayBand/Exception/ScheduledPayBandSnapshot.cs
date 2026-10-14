namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band snapshot is in its life cycle.</summary>
public enum ScheduledPayBandSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled pay band snapshot, with what the planners need to act on it.</summary>
public sealed class ScheduledPayBandSnapshot(string code, string name, int createdAt, string costCenter, double margin, int windowDays, double rating, double ceiling, int retries, List<string> tags, string jobCode, ScheduledPayBandSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string CostCenter { get; set; } = costCenter;
    public double Margin { get; set; } = margin;
    public int WindowDays { get; set; } = windowDays;
    public double Rating { get; set; } = rating;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public List<string> Tags { get; set; } = tags;
    public string JobCode { get; set; } = jobCode;
    public ScheduledPayBandSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band snapshot in status proposed.</summary>
    public static ScheduledPayBandSnapshot NewScheduledPayBandSnapshot(string code, string name)
    {
        return new ScheduledPayBandSnapshot(
            code,
            name,
            0,
            "",
            0.0,
            0,
            0.0,
            0.0,
            0,
            new List<string>(),
            "",
            ScheduledPayBandSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandSnapshotStatus.Proposed:
                return next == ScheduledPayBandSnapshotStatus.Withdrawn;
            case ScheduledPayBandSnapshotStatus.Approved:
                return next == ScheduledPayBandSnapshotStatus.Rejected;
            case ScheduledPayBandSnapshotStatus.Rejected:
                switch (next)
                {
                    case ScheduledPayBandSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double NetTotal()
    {
        return Math.Round((double)WindowDays - Margin, 2);
    }

    public ScheduledPayBandSnapshot Copy()
    {
        return new ScheduledPayBandSnapshot(
            Code,
            Name,
            CreatedAt,
            CostCenter,
            Margin,
            WindowDays,
            Rating,
            Ceiling,
            Retries,
            Tags,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
