namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool notice is in its life cycle.</summary>
public enum PoolNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pool notice and its current status.</summary>
public sealed class PoolNotice(string code, string label, string jobCode, string segment, Dictionary<string, string> attributes, string source, string category, double floorValue, PoolNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Source { get; set; } = source;
    public string Category { get; set; } = category;
    public double FloorValue { get; set; } = floorValue;
    public PoolNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a pool notice in status proposed.</summary>
    public static PoolNotice NewPoolNotice(string code, string label)
    {
        return new PoolNotice(
            code,
            label,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            PoolNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolNoticeStatus next)
    {
        switch (Status)
        {
            case PoolNoticeStatus.Proposed:
                return next == PoolNoticeStatus.Rejected;
            case PoolNoticeStatus.Approved:
                switch (next)
                {
                    case PoolNoticeStatus.Rejected:
                        return true;
                }
                return false;
            case PoolNoticeStatus.Rejected:
                switch (next)
                {
                    case PoolNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return FloorValue;
    }

    public PoolNotice Copy()
    {
        return new PoolNotice(Code, Label, JobCode, Segment, Attributes, Source, Category, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
