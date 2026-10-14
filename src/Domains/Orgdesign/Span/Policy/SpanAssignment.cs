namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span assignment is in its life cycle.</summary>
public enum SpanAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span assignment as the span team keeps it.</summary>
public sealed class SpanAssignment(string code, string name, int updatedAt, double minimum, int windowDays, double score, double budget, SpanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public SpanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a span assignment in status planned.</summary>
    public static SpanAssignment NewSpanAssignment(string code, string name)
    {
        return new SpanAssignment(code, name, 0, 0.0, 0, 0.0, 0.0, SpanAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanAssignmentStatus next)
    {
        switch (Status)
        {
            case SpanAssignmentStatus.Planned:
                return next == SpanAssignmentStatus.Completed;
            case SpanAssignmentStatus.Scheduled:
                switch (next)
                {
                    case SpanAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanAssignmentStatus.InProgress:
                switch (next)
                {
                    case SpanAssignmentStatus.Completed:
                        return true;
                }
                return false;
            case SpanAssignmentStatus.Completed:
                switch (next)
                {
                    case SpanAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt + Score, 2);
    }

    public SpanAssignment Copy()
    {
        return new SpanAssignment(Code, Name, UpdatedAt, Minimum, WindowDays, Score, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
