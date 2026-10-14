namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive benchmark segment is in its life cycle.</summary>
public enum AdaptiveBenchmarkSegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive benchmark segment. Its code is unique within a tenant.</summary>
public sealed class AdaptiveBenchmarkSegment(string code, string name, int updatedAt, string reviewer, bool visible, string grade, AdaptiveBenchmarkSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public AdaptiveBenchmarkSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive benchmark segment in status open.</summary>
    public static AdaptiveBenchmarkSegment NewAdaptiveBenchmarkSegment(string code, string name)
    {
        return new AdaptiveBenchmarkSegment(code, name, 0, "", false, "", AdaptiveBenchmarkSegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBenchmarkSegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBenchmarkSegmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveBenchmarkSegmentStatus.Open:
                return next == AdaptiveBenchmarkSegmentStatus.InReview || next == AdaptiveBenchmarkSegmentStatus.Resolved;
            case AdaptiveBenchmarkSegmentStatus.InReview:
                return next == AdaptiveBenchmarkSegmentStatus.Archived;
            case AdaptiveBenchmarkSegmentStatus.Resolved:
                switch (next)
                {
                    case AdaptiveBenchmarkSegmentStatus.Reopened:
                    case AdaptiveBenchmarkSegmentStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkSegmentStatus.Reopened:
                return next == AdaptiveBenchmarkSegmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveBenchmarkSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive benchmark segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public AdaptiveBenchmarkSegment Copy()
    {
        return new AdaptiveBenchmarkSegment(Code, Name, UpdatedAt, Reviewer, Visible, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
