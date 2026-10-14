namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding segment is in its life cycle.</summary>
public enum OffboardingSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A offboarding segment. Its code is unique within a tenant.</summary>
public sealed class OffboardingSegment(string code, string name, double ratio, string summary, double ceiling, int expiresAt, int retries, OffboardingSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Summary { get; set; } = summary;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public OffboardingSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding segment in status idle.</summary>
    public static OffboardingSegment NewOffboardingSegment(string code, string name)
    {
        return new OffboardingSegment(code, name, 0.0, "", 0.0, 0, 0, OffboardingSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingSegmentStatus next)
    {
        switch (Status)
        {
            case OffboardingSegmentStatus.Idle:
                return next == OffboardingSegmentStatus.Ready;
            case OffboardingSegmentStatus.Warming:
                switch (next)
                {
                    case OffboardingSegmentStatus.Stopped:
                        return true;
                }
                return false;
            case OffboardingSegmentStatus.Ready:
                switch (next)
                {
                    case OffboardingSegmentStatus.Draining:
                        return true;
                }
                return false;
            case OffboardingSegmentStatus.Draining:
                switch (next)
                {
                    case OffboardingSegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"offboarding segment"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Ratio * (double)Retries, 2);
    }

    public OffboardingSegment Copy()
    {
        return new OffboardingSegment(Code, Name, Ratio, Summary, Ceiling, ExpiresAt, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
