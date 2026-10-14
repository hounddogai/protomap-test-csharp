namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool segment is in its life cycle.</summary>
public enum PoolSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pool segment and its current status.</summary>
public sealed class PoolSegment(string code, string title, string salaryBand, double fte, int windowDays, string managerId, PoolSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public PoolSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a pool segment in status idle.</summary>
    public static PoolSegment NewPoolSegment(string code, string title)
    {
        return new PoolSegment(code, title, "", 0.0, 0, "", PoolSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolSegmentStatus next)
    {
        switch (Status)
        {
            case PoolSegmentStatus.Idle:
                switch (next)
                {
                    case PoolSegmentStatus.Draining:
                        return true;
                }
                return false;
            case PoolSegmentStatus.Warming:
                return next == PoolSegmentStatus.Ready;
            case PoolSegmentStatus.Ready:
                return next == PoolSegmentStatus.Draining || next == PoolSegmentStatus.Stopped;
            case PoolSegmentStatus.Draining:
                switch (next)
                {
                    case PoolSegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays - Fte, 2);
    }

    public PoolSegment Copy()
    {
        return new PoolSegment(Code, Title, SalaryBand, Fte, WindowDays, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
