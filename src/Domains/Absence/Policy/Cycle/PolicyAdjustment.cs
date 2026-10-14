namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy adjustment is in its life cycle.</summary>
public enum PolicyAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One policy adjustment, with what the planners need to act on it.</summary>
public sealed class PolicyAdjustment(string code, string title, double rate, List<string> tags, int batchSize, string unit, double amount, double budget, string segment, int capacity, PolicyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public PolicyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy adjustment in status open.</summary>
    public static PolicyAdjustment NewPolicyAdjustment(string code, string title)
    {
        return new PolicyAdjustment(
            code,
            title,
            0.0,
            new List<string>(),
            0,
            "",
            0.0,
            0.0,
            "",
            0,
            PolicyAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAdjustmentStatus next)
    {
        switch (Status)
        {
            case PolicyAdjustmentStatus.Open:
                switch (next)
                {
                    case PolicyAdjustmentStatus.Resolved:
                    case PolicyAdjustmentStatus.Reopened:
                        return true;
                }
                return false;
            case PolicyAdjustmentStatus.InReview:
                return next == PolicyAdjustmentStatus.Resolved || next == PolicyAdjustmentStatus.Reopened;
            case PolicyAdjustmentStatus.Resolved:
                switch (next)
                {
                    case PolicyAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case PolicyAdjustmentStatus.Reopened:
                return next == PolicyAdjustmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PolicyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double EffectiveValue()
    {
        return Math.Round(Budget * Amount, 2);
    }

    public PolicyAdjustment Copy()
    {
        return new PolicyAdjustment(
            Code,
            Title,
            Rate,
            Tags,
            BatchSize,
            Unit,
            Amount,
            Budget,
            Segment,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
