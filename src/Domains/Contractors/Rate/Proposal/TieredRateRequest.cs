namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered rate request is in its life cycle.</summary>
public enum TieredRateRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered rate request. Its code is unique within a tenant.</summary>
public sealed class TieredRateRequest(string code, string label, string team, double score, double margin, int durationMinutes, string employeeId, int priority, int endsAt, string currency, double maximum, TieredRateRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public double Margin { get; set; } = margin;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public TieredRateRequestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered rate request in status planned.</summary>
    public static TieredRateRequest NewTieredRateRequest(string code, string label)
    {
        return new TieredRateRequest(
            code,
            label,
            "",
            0.0,
            0.0,
            0,
            "",
            0,
            0,
            "",
            0.0,
            TieredRateRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredRateRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredRateRequestStatus next)
    {
        switch (Status)
        {
            case TieredRateRequestStatus.Planned:
                return next == TieredRateRequestStatus.Scheduled;
            case TieredRateRequestStatus.Scheduled:
                switch (next)
                {
                    case TieredRateRequestStatus.Completed:
                    case TieredRateRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredRateRequestStatus.InProgress:
                switch (next)
                {
                    case TieredRateRequestStatus.Completed:
                    case TieredRateRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredRateRequestStatus.Completed:
                switch (next)
                {
                    case TieredRateRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredRateRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered rate request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum - Score, 2);
    }

    public TieredRateRequest Copy()
    {
        return new TieredRateRequest(
            Code,
            Label,
            Team,
            Score,
            Margin,
            DurationMinutes,
            EmployeeId,
            Priority,
            EndsAt,
            Currency,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
