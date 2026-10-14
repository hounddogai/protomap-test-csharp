namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet variance is in its life cycle.</summary>
public enum TimesheetVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One timesheet variance, with what the planners need to act on it.</summary>
public sealed class TimesheetVariance(string code, string label, bool archived, double score, int createdAt, double rate, int limit, string reviewer, double fte, TimesheetVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public double Rate { get; set; } = rate;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public TimesheetVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet variance in status new.</summary>
    public static TimesheetVariance NewTimesheetVariance(string code, string label)
    {
        return new TimesheetVariance(code, label, false, 0.0, 0, 0.0, 0, "", 0.0, TimesheetVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetVarianceStatus next)
    {
        switch (Status)
        {
            case TimesheetVarianceStatus.New:
                return next == TimesheetVarianceStatus.Done;
            case TimesheetVarianceStatus.Queued:
                return next == TimesheetVarianceStatus.Done;
            case TimesheetVarianceStatus.Running:
                switch (next)
                {
                    case TimesheetVarianceStatus.Done:
                    case TimesheetVarianceStatus.Failed:
                        return true;
                }
                return false;
            case TimesheetVarianceStatus.Done:
                return next == TimesheetVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TimesheetVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"timesheet variance"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Limit + (double)CreatedAt, 2);
    }

    public TimesheetVariance Copy()
    {
        return new TimesheetVariance(Code, Label, Archived, Score, CreatedAt, Rate, Limit, Reviewer, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
