namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual baseline is in its life cycle.</summary>
public enum AccrualBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A accrual baseline as the accrual team keeps it.</summary>
public sealed class AccrualBaseline(string code, string title, int batchSize, int durationMinutes, string ownerId, AccrualBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public AccrualBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a accrual baseline in status proposed.</summary>
    public static AccrualBaseline NewAccrualBaseline(string code, string title)
    {
        return new AccrualBaseline(code, title, 0, 0, "", AccrualBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualBaselineStatus next)
    {
        switch (Status)
        {
            case AccrualBaselineStatus.Proposed:
                return next == AccrualBaselineStatus.Rejected || next == AccrualBaselineStatus.Withdrawn;
            case AccrualBaselineStatus.Approved:
                switch (next)
                {
                    case AccrualBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case AccrualBaselineStatus.Rejected:
                return next == AccrualBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AccrualBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"accrual baseline"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes - (double)BatchSize, 2);
    }

    public AccrualBaseline Copy()
    {
        return new AccrualBaseline(Code, Title, BatchSize, DurationMinutes, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
