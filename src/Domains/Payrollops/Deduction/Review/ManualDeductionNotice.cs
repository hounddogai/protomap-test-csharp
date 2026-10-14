namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual deduction notice is in its life cycle.</summary>
public enum ManualDeductionNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A manual deduction notice as the deduction team keeps it.</summary>
public sealed class ManualDeductionNotice(string code, string title, bool archived, int leaveDays, double rate, int durationMinutes, string unit, double threshold, ManualDeductionNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Unit { get; set; } = unit;
    public double Threshold { get; set; } = threshold;
    public ManualDeductionNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a manual deduction notice in status new.</summary>
    public static ManualDeductionNotice NewManualDeductionNotice(string code, string title)
    {
        return new ManualDeductionNotice(code, title, false, 0, 0.0, 0, "", 0.0, ManualDeductionNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualDeductionNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualDeductionNoticeStatus next)
    {
        switch (Status)
        {
            case ManualDeductionNoticeStatus.New:
                return next == ManualDeductionNoticeStatus.Running || next == ManualDeductionNoticeStatus.Failed;
            case ManualDeductionNoticeStatus.Queued:
                switch (next)
                {
                    case ManualDeductionNoticeStatus.Running:
                    case ManualDeductionNoticeStatus.Failed:
                        return true;
                }
                return false;
            case ManualDeductionNoticeStatus.Running:
                return next == ManualDeductionNoticeStatus.Done || next == ManualDeductionNoticeStatus.Failed;
            case ManualDeductionNoticeStatus.Done:
                switch (next)
                {
                    case ManualDeductionNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualDeductionNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual deduction notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual deduction notice"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes - Rate, 2);
    }

    public ManualDeductionNotice Copy()
    {
        return new ManualDeductionNotice(
            Code,
            Title,
            Archived,
            LeaveDays,
            Rate,
            DurationMinutes,
            Unit,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
