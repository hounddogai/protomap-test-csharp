namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span metric is in its life cycle.</summary>
public enum SpanMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One span metric, with what the planners need to act on it.</summary>
public sealed class SpanMetric(string code, string name, double fte, Dictionary<string, string> attributes, double cost, string department, string badge, string grade, SpanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public string Department { get; set; } = department;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public SpanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a span metric in status open.</summary>
    public static SpanMetric NewSpanMetric(string code, string name)
    {
        return new SpanMetric(
            code,
            name,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            SpanMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanMetricStatus next)
    {
        switch (Status)
        {
            case SpanMetricStatus.Open:
                switch (next)
                {
                    case SpanMetricStatus.Reopened:
                        return true;
                }
                return false;
            case SpanMetricStatus.InReview:
                return next == SpanMetricStatus.Resolved || next == SpanMetricStatus.Reopened;
            case SpanMetricStatus.Resolved:
                return next == SpanMetricStatus.Reopened || next == SpanMetricStatus.Archived;
            case SpanMetricStatus.Reopened:
                return next == SpanMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SpanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double ProjectedLoad()
    {
        return Math.Round(Cost * Fte, 2);
    }

    public SpanMetric Copy()
    {
        return new SpanMetric(Code, Name, Fte, Attributes, Cost, Department, Badge, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
