namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy occupancy exception is in its life cycle.</summary>
public enum LegacyOccupancyExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A legacy occupancy exception. Its code is unique within a tenant.</summary>
public sealed class LegacyOccupancyException(string code, string title, int graceDays, string label, int limit, LegacyOccupancyExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public LegacyOccupancyExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a legacy occupancy exception in status draft.</summary>
    public static LegacyOccupancyException NewLegacyOccupancyException(string code, string title)
    {
        return new LegacyOccupancyException(code, title, 0, "", 0, LegacyOccupancyExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOccupancyExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOccupancyExceptionStatus next)
    {
        switch (Status)
        {
            case LegacyOccupancyExceptionStatus.Draft:
                return next == LegacyOccupancyExceptionStatus.Closed;
            case LegacyOccupancyExceptionStatus.Pending:
                return next == LegacyOccupancyExceptionStatus.Closed;
            case LegacyOccupancyExceptionStatus.Active:
                return next == LegacyOccupancyExceptionStatus.Suspended || next == LegacyOccupancyExceptionStatus.Closed;
            case LegacyOccupancyExceptionStatus.Suspended:
                return next == LegacyOccupancyExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyOccupancyExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy occupancy exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays * (double)Limit, 2);
    }

    public LegacyOccupancyException Copy()
    {
        return new LegacyOccupancyException(Code, Title, GraceDays, Label, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
