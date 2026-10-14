namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default benchmark rollup is in its life cycle.</summary>
public enum DefaultBenchmarkRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default benchmark rollup and its current status.</summary>
public sealed class DefaultBenchmarkRollup(string code, string label, Dictionary<string, string> attributes, string segment, string grade, int capacity, string reviewer, DefaultBenchmarkRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Segment { get; set; } = segment;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public DefaultBenchmarkRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default benchmark rollup in status planned.</summary>
    public static DefaultBenchmarkRollup NewDefaultBenchmarkRollup(string code, string label)
    {
        return new DefaultBenchmarkRollup(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            DefaultBenchmarkRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBenchmarkRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBenchmarkRollupStatus next)
    {
        switch (Status)
        {
            case DefaultBenchmarkRollupStatus.Planned:
                switch (next)
                {
                    case DefaultBenchmarkRollupStatus.Scheduled:
                        return true;
                }
                return false;
            case DefaultBenchmarkRollupStatus.Scheduled:
                switch (next)
                {
                    case DefaultBenchmarkRollupStatus.InProgress:
                        return true;
                }
                return false;
            case DefaultBenchmarkRollupStatus.InProgress:
                return next == DefaultBenchmarkRollupStatus.Cancelled;
            case DefaultBenchmarkRollupStatus.Completed:
                return next == DefaultBenchmarkRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultBenchmarkRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default benchmark rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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
        return (double)Capacity;
    }

    public DefaultBenchmarkRollup Copy()
    {
        return new DefaultBenchmarkRollup(Code, Label, Attributes, Segment, Grade, Capacity, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
