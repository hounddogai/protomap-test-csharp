namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span mapping is in its life cycle.</summary>
public enum SpanMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One span mapping, with what the planners need to act on it.</summary>
public sealed class SpanMapping(string code, string label, Dictionary<string, string> attributes, string team, bool active, string channel, int leaveDays, SpanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public bool Active { get; set; } = active;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public SpanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a span mapping in status planned.</summary>
    public static SpanMapping NewSpanMapping(string code, string label)
    {
        return new SpanMapping(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            false,
            "",
            0,
            SpanMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanMappingStatus next)
    {
        switch (Status)
        {
            case SpanMappingStatus.Planned:
                return next == SpanMappingStatus.InProgress || next == SpanMappingStatus.Completed;
            case SpanMappingStatus.Scheduled:
                switch (next)
                {
                    case SpanMappingStatus.Completed:
                        return true;
                }
                return false;
            case SpanMappingStatus.InProgress:
                return next == SpanMappingStatus.Completed || next == SpanMappingStatus.Cancelled;
            case SpanMappingStatus.Completed:
                return next == SpanMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span mapping"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public SpanMapping Copy()
    {
        return new SpanMapping(Code, Label, Attributes, Team, Active, Channel, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
