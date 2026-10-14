namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span group is in its life cycle.</summary>
public enum SpanGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A span group as the span team keeps it.</summary>
public sealed class SpanGroup(string code, string label, string skill, string department, bool approved, double maximum, double amount, double ceiling, double floorValue, string externalRef, SpanGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Department { get; set; } = department;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public double Ceiling { get; set; } = ceiling;
    public double FloorValue { get; set; } = floorValue;
    public string ExternalRef { get; set; } = externalRef;
    public SpanGroupStatus Status { get; set; } = status;

    /// <summary>Starts a span group in status idle.</summary>
    public static SpanGroup NewSpanGroup(string code, string label)
    {
        return new SpanGroup(code, label, "", "", false, 0.0, 0.0, 0.0, 0.0, "", SpanGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanGroupStatus next)
    {
        switch (Status)
        {
            case SpanGroupStatus.Idle:
                switch (next)
                {
                    case SpanGroupStatus.Ready:
                    case SpanGroupStatus.Stopped:
                        return true;
                }
                return false;
            case SpanGroupStatus.Warming:
                switch (next)
                {
                    case SpanGroupStatus.Draining:
                    case SpanGroupStatus.Stopped:
                        return true;
                }
                return false;
            case SpanGroupStatus.Ready:
                return next == SpanGroupStatus.Draining || next == SpanGroupStatus.Stopped;
            case SpanGroupStatus.Draining:
                switch (next)
                {
                    case SpanGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span group"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(FloorValue * Maximum, 2);
    }

    public SpanGroup Copy()
    {
        return new SpanGroup(
            Code,
            Label,
            Skill,
            Department,
            Approved,
            Maximum,
            Amount,
            Ceiling,
            FloorValue,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
