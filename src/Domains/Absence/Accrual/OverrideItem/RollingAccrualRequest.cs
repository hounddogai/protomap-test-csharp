namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling accrual request is in its life cycle.</summary>
public enum RollingAccrualRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling accrual request, with what the planners need to act on it.</summary>
public sealed class RollingAccrualRequest(string code, string label, double margin, string country, bool archived, int durationMinutes, double score, string currency, bool visible, int batchSize, double hours, RollingAccrualRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public RollingAccrualRequestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling accrual request in status planned.</summary>
    public static RollingAccrualRequest NewRollingAccrualRequest(string code, string label)
    {
        return new RollingAccrualRequest(
            code,
            label,
            0.0,
            "",
            false,
            0,
            0.0,
            "",
            false,
            0,
            0.0,
            RollingAccrualRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccrualRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccrualRequestStatus next)
    {
        switch (Status)
        {
            case RollingAccrualRequestStatus.Planned:
                switch (next)
                {
                    case RollingAccrualRequestStatus.InProgress:
                        return true;
                }
                return false;
            case RollingAccrualRequestStatus.Scheduled:
                switch (next)
                {
                    case RollingAccrualRequestStatus.Completed:
                    case RollingAccrualRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingAccrualRequestStatus.InProgress:
                switch (next)
                {
                    case RollingAccrualRequestStatus.Completed:
                        return true;
                }
                return false;
            case RollingAccrualRequestStatus.Completed:
                switch (next)
                {
                    case RollingAccrualRequestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccrualRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling accrual request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Score + Margin, 2);
    }

    public RollingAccrualRequest Copy()
    {
        return new RollingAccrualRequest(
            Code,
            Label,
            Margin,
            Country,
            Archived,
            DurationMinutes,
            Score,
            Currency,
            Visible,
            BatchSize,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
