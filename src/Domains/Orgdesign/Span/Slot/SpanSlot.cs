namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span slot is in its life cycle.</summary>
public enum SpanSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A span slot as the span team keeps it.</summary>
public sealed class SpanSlot(string code, string label, double score, Dictionary<string, string> attributes, int attempts, bool approved, double ceiling, int endsAt, int version, SpanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public bool Approved { get; set; } = approved;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public SpanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a span slot in status planned.</summary>
    public static SpanSlot NewSpanSlot(string code, string label)
    {
        return new SpanSlot(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            0,
            false,
            0.0,
            0,
            0,
            SpanSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanSlotStatus next)
    {
        switch (Status)
        {
            case SpanSlotStatus.Planned:
                return next == SpanSlotStatus.InProgress;
            case SpanSlotStatus.Scheduled:
                switch (next)
                {
                    case SpanSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case SpanSlotStatus.InProgress:
                return next == SpanSlotStatus.Completed;
            case SpanSlotStatus.Completed:
                return next == SpanSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SpanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"span slot"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt * (double)Version, 2);
    }

    public SpanSlot Copy()
    {
        return new SpanSlot(Code, Label, Score, Attributes, Attempts, Approved, Ceiling, EndsAt, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
