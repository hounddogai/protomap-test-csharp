namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span batch is in its life cycle.</summary>
public enum SpanBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A span batch as the span team keeps it.</summary>
public sealed class SpanBatch(string code, string label, string currency, bool active, double budget, bool visible, string name, Dictionary<string, string> attributes, string department, SpanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public bool Active { get; set; } = active;
    public double Budget { get; set; } = budget;
    public bool Visible { get; set; } = visible;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public SpanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a span batch in status proposed.</summary>
    public static SpanBatch NewSpanBatch(string code, string label)
    {
        return new SpanBatch(
            code,
            label,
            "",
            false,
            0.0,
            false,
            "",
            new Dictionary<string, string>(),
            "",
            SpanBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanBatchStatus next)
    {
        switch (Status)
        {
            case SpanBatchStatus.Proposed:
                return next == SpanBatchStatus.Approved || next == SpanBatchStatus.Withdrawn;
            case SpanBatchStatus.Approved:
                return next == SpanBatchStatus.Withdrawn;
            case SpanBatchStatus.Rejected:
                switch (next)
                {
                    case SpanBatchStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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
        return Budget;
    }

    public SpanBatch Copy()
    {
        return new SpanBatch(Code, Label, Currency, Active, Budget, Visible, Name, Attributes, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
