namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit rollup is in its life cycle.</summary>
public enum AuditRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a audit rollup and its current status.</summary>
public sealed class AuditRollup(string code, string title, double minimum, int updatedAt, int revision, bool active, int startsAt, string category, double ceiling, double margin, AuditRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public bool Active { get; set; } = active;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public double Ceiling { get; set; } = ceiling;
    public double Margin { get; set; } = margin;
    public AuditRollupStatus Status { get; set; } = status;

    /// <summary>Starts a audit rollup in status planned.</summary>
    public static AuditRollup NewAuditRollup(string code, string title)
    {
        return new AuditRollup(code, title, 0.0, 0, 0, false, 0, "", 0.0, 0.0, AuditRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditRollupStatus next)
    {
        switch (Status)
        {
            case AuditRollupStatus.Planned:
                return next == AuditRollupStatus.Completed || next == AuditRollupStatus.Cancelled;
            case AuditRollupStatus.Scheduled:
                switch (next)
                {
                    case AuditRollupStatus.InProgress:
                    case AuditRollupStatus.Completed:
                        return true;
                }
                return false;
            case AuditRollupStatus.InProgress:
                return next == AuditRollupStatus.Completed;
            case AuditRollupStatus.Completed:
                switch (next)
                {
                    case AuditRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"audit rollup"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling + (double)Revision, 2);
    }

    public AuditRollup Copy()
    {
        return new AuditRollup(
            Code,
            Title,
            Minimum,
            UpdatedAt,
            Revision,
            Active,
            StartsAt,
            Category,
            Ceiling,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
