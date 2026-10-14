namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted accrual request is in its life cycle.</summary>
public enum WeightedAccrualRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted accrual request. Its code is unique within a tenant.</summary>
public sealed class WeightedAccrualRequest(string code, string title, double hours, string checksum, int createdAt, WeightedAccrualRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public WeightedAccrualRequestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted accrual request in status planned.</summary>
    public static WeightedAccrualRequest NewWeightedAccrualRequest(string code, string title)
    {
        return new WeightedAccrualRequest(code, title, 0.0, "", 0, WeightedAccrualRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccrualRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccrualRequestStatus next)
    {
        switch (Status)
        {
            case WeightedAccrualRequestStatus.Planned:
                return next == WeightedAccrualRequestStatus.Cancelled;
            case WeightedAccrualRequestStatus.Scheduled:
                return next == WeightedAccrualRequestStatus.Completed || next == WeightedAccrualRequestStatus.Cancelled;
            case WeightedAccrualRequestStatus.InProgress:
                return next == WeightedAccrualRequestStatus.Completed || next == WeightedAccrualRequestStatus.Cancelled;
            case WeightedAccrualRequestStatus.Completed:
                return next == WeightedAccrualRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedAccrualRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted accrual request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)CreatedAt - Hours, 2);
    }

    public WeightedAccrualRequest Copy()
    {
        return new WeightedAccrualRequest(Code, Title, Hours, Checksum, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
