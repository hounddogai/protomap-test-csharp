namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span rollup is in its life cycle.</summary>
public enum SpanRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A span rollup as the span team keeps it.</summary>
public sealed class SpanRollup(string code, string name, double score, Dictionary<string, string> attributes, int version, SpanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public SpanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a span rollup in status open.</summary>
    public static SpanRollup NewSpanRollup(string code, string name)
    {
        return new SpanRollup(code, name, 0.0, new Dictionary<string, string>(), 0, SpanRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanRollupStatus next)
    {
        switch (Status)
        {
            case SpanRollupStatus.Open:
                switch (next)
                {
                    case SpanRollupStatus.InReview:
                    case SpanRollupStatus.Archived:
                        return true;
                }
                return false;
            case SpanRollupStatus.InReview:
                return next == SpanRollupStatus.Resolved || next == SpanRollupStatus.Reopened;
            case SpanRollupStatus.Resolved:
                return next == SpanRollupStatus.Reopened || next == SpanRollupStatus.Archived;
            case SpanRollupStatus.Reopened:
                return next == SpanRollupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SpanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"span rollup"} {Code}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Version - Score, 2);
    }

    public SpanRollup Copy()
    {
        return new SpanRollup(Code, Name, Score, Attributes, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
