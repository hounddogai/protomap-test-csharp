namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted laptop audit is in its life cycle.</summary>
public enum WeightedLaptopAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted laptop audit as the laptop team keeps it.</summary>
public sealed class WeightedLaptopAudit(string code, string label, string notes, int durationMinutes, string reviewer, double score, string region, int expiresAt, double hours, string category, WeightedLaptopAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public string Region { get; set; } = region;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public string Category { get; set; } = category;
    public WeightedLaptopAuditStatus Status { get; set; } = status;

    /// <summary>Starts a weighted laptop audit in status new.</summary>
    public static WeightedLaptopAudit NewWeightedLaptopAudit(string code, string label)
    {
        return new WeightedLaptopAudit(code, label, "", 0, "", 0.0, "", 0, 0.0, "", WeightedLaptopAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedLaptopAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedLaptopAuditStatus next)
    {
        switch (Status)
        {
            case WeightedLaptopAuditStatus.New:
                switch (next)
                {
                    case WeightedLaptopAuditStatus.Queued:
                    case WeightedLaptopAuditStatus.Running:
                        return true;
                }
                return false;
            case WeightedLaptopAuditStatus.Queued:
                return next == WeightedLaptopAuditStatus.Failed;
            case WeightedLaptopAuditStatus.Running:
                switch (next)
                {
                    case WeightedLaptopAuditStatus.Done:
                    case WeightedLaptopAuditStatus.Failed:
                        return true;
                }
                return false;
            case WeightedLaptopAuditStatus.Done:
                switch (next)
                {
                    case WeightedLaptopAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedLaptopAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted laptop audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"weighted laptop audit"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt - Score, 2);
    }

    public WeightedLaptopAudit Copy()
    {
        return new WeightedLaptopAudit(
            Code,
            Label,
            Notes,
            DurationMinutes,
            Reviewer,
            Score,
            Region,
            ExpiresAt,
            Hours,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
