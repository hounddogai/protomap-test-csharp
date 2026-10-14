namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict scenario quota is in its life cycle.</summary>
public enum StrictScenarioQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict scenario quota as the scenario team keeps it.</summary>
public sealed class StrictScenarioQuota(string code, string name, int startsAt, int createdAt, int durationMinutes, int sequence, string country, string segment, bool pinned, bool active, StrictScenarioQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Sequence { get; set; } = sequence;
    public string Country { get; set; } = country;
    public string Segment { get; set; } = segment;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public StrictScenarioQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict scenario quota in status planned.</summary>
    public static StrictScenarioQuota NewStrictScenarioQuota(string code, string name)
    {
        return new StrictScenarioQuota(
            code,
            name,
            0,
            0,
            0,
            0,
            "",
            "",
            false,
            false,
            StrictScenarioQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictScenarioQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictScenarioQuotaStatus next)
    {
        switch (Status)
        {
            case StrictScenarioQuotaStatus.Planned:
                return next == StrictScenarioQuotaStatus.Scheduled;
            case StrictScenarioQuotaStatus.Scheduled:
                switch (next)
                {
                    case StrictScenarioQuotaStatus.Completed:
                        return true;
                }
                return false;
            case StrictScenarioQuotaStatus.InProgress:
                switch (next)
                {
                    case StrictScenarioQuotaStatus.Completed:
                    case StrictScenarioQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictScenarioQuotaStatus.Completed:
                return next == StrictScenarioQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictScenarioQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict scenario quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Sequence - (double)CreatedAt, 2);
    }

    public StrictScenarioQuota Copy()
    {
        return new StrictScenarioQuota(
            Code,
            Name,
            StartsAt,
            CreatedAt,
            DurationMinutes,
            Sequence,
            Country,
            Segment,
            Pinned,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
