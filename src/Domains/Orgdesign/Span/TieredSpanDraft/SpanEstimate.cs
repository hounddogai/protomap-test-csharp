namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span estimate is in its life cycle.</summary>
public enum SpanEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A span estimate. Its code is unique within a tenant.</summary>
public sealed class SpanEstimate(string code, string name, int windowDays, string checksum, int updatedAt, string badge, int durationMinutes, string department, string managerId, string reviewer, int revision, SpanEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Badge { get; set; } = badge;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public string ManagerId { get; set; } = managerId;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public SpanEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a span estimate in status idle.</summary>
    public static SpanEstimate NewSpanEstimate(string code, string name)
    {
        return new SpanEstimate(code, name, 0, "", 0, "", 0, "", "", "", 0, SpanEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanEstimateStatus next)
    {
        switch (Status)
        {
            case SpanEstimateStatus.Idle:
                switch (next)
                {
                    case SpanEstimateStatus.Draining:
                    case SpanEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case SpanEstimateStatus.Warming:
                switch (next)
                {
                    case SpanEstimateStatus.Draining:
                        return true;
                }
                return false;
            case SpanEstimateStatus.Ready:
                return next == SpanEstimateStatus.Stopped;
            case SpanEstimateStatus.Draining:
                switch (next)
                {
                    case SpanEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"span estimate"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)WindowDays * (double)UpdatedAt, 2);
    }

    public SpanEstimate Copy()
    {
        return new SpanEstimate(
            Code,
            Name,
            WindowDays,
            Checksum,
            UpdatedAt,
            Badge,
            DurationMinutes,
            Department,
            ManagerId,
            Reviewer,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
