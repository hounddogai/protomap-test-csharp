namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool record is in its life cycle.</summary>
public enum PoolRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pool record, with what the planners need to act on it.</summary>
public sealed class PoolRecord(string code, string title, double budget, string reviewer, int tenureMonths, int capacity, bool visible, bool approved, PoolRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public bool Visible { get; set; } = visible;
    public bool Approved { get; set; } = approved;
    public PoolRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pool record in status created.</summary>
    public static PoolRecord NewPoolRecord(string code, string title)
    {
        return new PoolRecord(code, title, 0.0, "", 0, 0, false, false, PoolRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolRecordStatus next)
    {
        switch (Status)
        {
            case PoolRecordStatus.Created:
                switch (next)
                {
                    case PoolRecordStatus.Published:
                    case PoolRecordStatus.Retired:
                        return true;
                }
                return false;
            case PoolRecordStatus.Validated:
                switch (next)
                {
                    case PoolRecordStatus.Published:
                    case PoolRecordStatus.Retired:
                        return true;
                }
                return false;
            case PoolRecordStatus.Published:
                return next == PoolRecordStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pool record"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity * Budget, 2);
    }

    public PoolRecord Copy()
    {
        return new PoolRecord(Code, Title, Budget, Reviewer, TenureMonths, Capacity, Visible, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
