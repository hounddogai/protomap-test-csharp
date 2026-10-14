namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered span schedule is in its life cycle.</summary>
public enum TieredSpanScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered span schedule. Its code is unique within a tenant.</summary>
public sealed class TieredSpanSchedule(string code, string name, double budget, string jobCode, string notes, int limit, TieredSpanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public TieredSpanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered span schedule in status idle.</summary>
    public static TieredSpanSchedule NewTieredSpanSchedule(string code, string name)
    {
        return new TieredSpanSchedule(code, name, 0.0, "", "", 0, TieredSpanScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSpanScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSpanScheduleStatus next)
    {
        switch (Status)
        {
            case TieredSpanScheduleStatus.Idle:
                return next == TieredSpanScheduleStatus.Ready;
            case TieredSpanScheduleStatus.Warming:
                switch (next)
                {
                    case TieredSpanScheduleStatus.Draining:
                    case TieredSpanScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case TieredSpanScheduleStatus.Ready:
                return next == TieredSpanScheduleStatus.Draining || next == TieredSpanScheduleStatus.Stopped;
            case TieredSpanScheduleStatus.Draining:
                switch (next)
                {
                    case TieredSpanScheduleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSpanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered span schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Budget - (double)Limit, 2);
    }

    public TieredSpanSchedule Copy()
    {
        return new TieredSpanSchedule(Code, Name, Budget, JobCode, Notes, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
