namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient booking plan is in its life cycle.</summary>
public enum LenientBookingPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient booking plan and its current status.</summary>
public sealed class LenientBookingPlan(string code, string name, int attempts, bool visible, int windowDays, string segment, bool archived, double score, double amount, LenientBookingPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public bool Visible { get; set; } = visible;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public bool Archived { get; set; } = archived;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public LenientBookingPlanStatus Status { get; set; } = status;

    /// <summary>Starts a lenient booking plan in status new.</summary>
    public static LenientBookingPlan NewLenientBookingPlan(string code, string name)
    {
        return new LenientBookingPlan(code, name, 0, false, 0, "", false, 0.0, 0.0, LenientBookingPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBookingPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBookingPlanStatus next)
    {
        switch (Status)
        {
            case LenientBookingPlanStatus.New:
                switch (next)
                {
                    case LenientBookingPlanStatus.Running:
                        return true;
                }
                return false;
            case LenientBookingPlanStatus.Queued:
                return next == LenientBookingPlanStatus.Running || next == LenientBookingPlanStatus.Failed;
            case LenientBookingPlanStatus.Running:
                switch (next)
                {
                    case LenientBookingPlanStatus.Failed:
                        return true;
                }
                return false;
            case LenientBookingPlanStatus.Done:
                switch (next)
                {
                    case LenientBookingPlanStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBookingPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient booking plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount * (double)WindowDays, 2);
    }

    public LenientBookingPlan Copy()
    {
        return new LenientBookingPlan(
            Code,
            Name,
            Attempts,
            Visible,
            WindowDays,
            Segment,
            Archived,
            Score,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
