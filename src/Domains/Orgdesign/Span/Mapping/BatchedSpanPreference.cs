namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched span preference is in its life cycle.</summary>
public enum BatchedSpanPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched span preference, with what the planners need to act on it.</summary>
public sealed class BatchedSpanPreference(string code, string name, int capacity, int priority, double maximum, double minimum, BatchedSpanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public double Minimum { get; set; } = minimum;
    public BatchedSpanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a batched span preference in status open.</summary>
    public static BatchedSpanPreference NewBatchedSpanPreference(string code, string name)
    {
        return new BatchedSpanPreference(code, name, 0, 0, 0.0, 0.0, BatchedSpanPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSpanPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSpanPreferenceStatus next)
    {
        switch (Status)
        {
            case BatchedSpanPreferenceStatus.Open:
                return next == BatchedSpanPreferenceStatus.Resolved;
            case BatchedSpanPreferenceStatus.InReview:
                switch (next)
                {
                    case BatchedSpanPreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case BatchedSpanPreferenceStatus.Resolved:
                switch (next)
                {
                    case BatchedSpanPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedSpanPreferenceStatus.Reopened:
                return next == BatchedSpanPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedSpanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched span preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched span preference"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Priority * Maximum, 2);
    }

    public BatchedSpanPreference Copy()
    {
        return new BatchedSpanPreference(Code, Name, Capacity, Priority, Maximum, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
