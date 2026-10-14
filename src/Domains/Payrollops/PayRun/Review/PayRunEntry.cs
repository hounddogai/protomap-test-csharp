namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run entry is in its life cycle.</summary>
public enum PayRunEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pay run entry, with what the planners need to act on it.</summary>
public sealed class PayRunEntry(string code, string title, bool active, double score, double floorValue, int graceDays, PayRunEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public int GraceDays { get; set; } = graceDays;
    public PayRunEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pay run entry in status new.</summary>
    public static PayRunEntry NewPayRunEntry(string code, string title)
    {
        return new PayRunEntry(code, title, false, 0.0, 0.0, 0, PayRunEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunEntryStatus next)
    {
        switch (Status)
        {
            case PayRunEntryStatus.New:
                switch (next)
                {
                    case PayRunEntryStatus.Running:
                    case PayRunEntryStatus.Failed:
                        return true;
                }
                return false;
            case PayRunEntryStatus.Queued:
                return next == PayRunEntryStatus.Running || next == PayRunEntryStatus.Done;
            case PayRunEntryStatus.Running:
                return next == PayRunEntryStatus.Failed;
            case PayRunEntryStatus.Done:
                switch (next)
                {
                    case PayRunEntryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay run entry"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)GraceDays + FloorValue, 2);
    }

    public PayRunEntry Copy()
    {
        return new PayRunEntry(Code, Title, Active, Score, FloorValue, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
