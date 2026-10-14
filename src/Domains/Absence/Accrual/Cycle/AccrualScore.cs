namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual score is in its life cycle.</summary>
public enum AccrualScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A accrual score as the accrual team keeps it.</summary>
public sealed class AccrualScore(string code, string title, string source, string badge, int priority, string notes, AccrualScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Badge { get; set; } = badge;
    public int Priority { get; set; } = priority;
    public string Notes { get; set; } = notes;
    public AccrualScoreStatus Status { get; set; } = status;

    /// <summary>Starts a accrual score in status idle.</summary>
    public static AccrualScore NewAccrualScore(string code, string title)
    {
        return new AccrualScore(code, title, "", "", 0, "", AccrualScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualScoreStatus next)
    {
        switch (Status)
        {
            case AccrualScoreStatus.Idle:
                switch (next)
                {
                    case AccrualScoreStatus.Draining:
                    case AccrualScoreStatus.Stopped:
                        return true;
                }
                return false;
            case AccrualScoreStatus.Warming:
                return next == AccrualScoreStatus.Stopped;
            case AccrualScoreStatus.Ready:
                return next == AccrualScoreStatus.Draining;
            case AccrualScoreStatus.Draining:
                switch (next)
                {
                    case AccrualScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public AccrualScore Copy()
    {
        return new AccrualScore(Code, Title, Source, Badge, Priority, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
