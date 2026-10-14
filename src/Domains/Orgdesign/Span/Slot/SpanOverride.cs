namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span override is in its life cycle.</summary>
public enum SpanOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A span override as the span team keeps it.</summary>
public sealed class SpanOverride(string code, string label, int leaveDays, string source, int graceDays, bool approved, Dictionary<string, string> attributes, string employeeId, List<string> tags, double fte, double minimum, SpanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public double Fte { get; set; } = fte;
    public double Minimum { get; set; } = minimum;
    public SpanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a span override in status new.</summary>
    public static SpanOverride NewSpanOverride(string code, string label)
    {
        return new SpanOverride(
            code,
            label,
            0,
            "",
            0,
            false,
            new Dictionary<string, string>(),
            "",
            new List<string>(),
            0.0,
            0.0,
            SpanOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanOverrideStatus next)
    {
        switch (Status)
        {
            case SpanOverrideStatus.New:
                return next == SpanOverrideStatus.Failed;
            case SpanOverrideStatus.Queued:
                return next == SpanOverrideStatus.Done;
            case SpanOverrideStatus.Running:
                return next == SpanOverrideStatus.Done;
            case SpanOverrideStatus.Done:
                switch (next)
                {
                    case SpanOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span override"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedValue()
    {
        return Math.Round((double)GraceDays + Fte, 2);
    }

    public SpanOverride Copy()
    {
        return new SpanOverride(
            Code,
            Label,
            LeaveDays,
            Source,
            GraceDays,
            Approved,
            Attributes,
            EmployeeId,
            Tags,
            Fte,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
