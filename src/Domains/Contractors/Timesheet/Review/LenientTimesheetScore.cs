namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient timesheet score is in its life cycle.</summary>
public enum LenientTimesheetScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One lenient timesheet score, with what the planners need to act on it.</summary>
public sealed class LenientTimesheetScore(string code, string label, string description, int windowDays, bool visible, double amount, double score, int updatedAt, int limit, LenientTimesheetScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public double Score { get; set; } = score;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public LenientTimesheetScoreStatus Status { get; set; } = status;

    /// <summary>Starts a lenient timesheet score in status idle.</summary>
    public static LenientTimesheetScore NewLenientTimesheetScore(string code, string label)
    {
        return new LenientTimesheetScore(
            code,
            label,
            "",
            0,
            false,
            0.0,
            0.0,
            0,
            0,
            LenientTimesheetScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTimesheetScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTimesheetScoreStatus next)
    {
        switch (Status)
        {
            case LenientTimesheetScoreStatus.Idle:
                return next == LenientTimesheetScoreStatus.Stopped;
            case LenientTimesheetScoreStatus.Warming:
                switch (next)
                {
                    case LenientTimesheetScoreStatus.Ready:
                    case LenientTimesheetScoreStatus.Stopped:
                        return true;
                }
                return false;
            case LenientTimesheetScoreStatus.Ready:
                switch (next)
                {
                    case LenientTimesheetScoreStatus.Draining:
                    case LenientTimesheetScoreStatus.Stopped:
                        return true;
                }
                return false;
            case LenientTimesheetScoreStatus.Draining:
                return next == LenientTimesheetScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientTimesheetScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient timesheet score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient timesheet score"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Amount * Score, 2);
    }

    public LenientTimesheetScore Copy()
    {
        return new LenientTimesheetScore(
            Code,
            Label,
            Description,
            WindowDays,
            Visible,
            Amount,
            Score,
            UpdatedAt,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
