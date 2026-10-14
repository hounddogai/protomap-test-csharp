namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling sentiment plan is in its life cycle.</summary>
public enum RollingSentimentPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling sentiment plan. Its code is unique within a tenant.</summary>
public sealed class RollingSentimentPlan(string code, string name, double threshold, double budget, double minimum, string reviewer, int retries, Dictionary<string, string> attributes, string unit, int batchSize, string description, RollingSentimentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Budget { get; set; } = budget;
    public double Minimum { get; set; } = minimum;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Unit { get; set; } = unit;
    public int BatchSize { get; set; } = batchSize;
    public string Description { get; set; } = description;
    public RollingSentimentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a rolling sentiment plan in status draft.</summary>
    public static RollingSentimentPlan NewRollingSentimentPlan(string code, string name)
    {
        return new RollingSentimentPlan(
            code,
            name,
            0.0,
            0.0,
            0.0,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            RollingSentimentPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSentimentPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSentimentPlanStatus next)
    {
        switch (Status)
        {
            case RollingSentimentPlanStatus.Draft:
                return next == RollingSentimentPlanStatus.Pending || next == RollingSentimentPlanStatus.Closed;
            case RollingSentimentPlanStatus.Pending:
                return next == RollingSentimentPlanStatus.Active || next == RollingSentimentPlanStatus.Closed;
            case RollingSentimentPlanStatus.Active:
                switch (next)
                {
                    case RollingSentimentPlanStatus.Suspended:
                        return true;
                }
                return false;
            case RollingSentimentPlanStatus.Suspended:
                return next == RollingSentimentPlanStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingSentimentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling sentiment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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
        return Math.Round(Minimum * (double)Retries, 2);
    }

    public RollingSentimentPlan Copy()
    {
        return new RollingSentimentPlan(
            Code,
            Name,
            Threshold,
            Budget,
            Minimum,
            Reviewer,
            Retries,
            Attributes,
            Unit,
            BatchSize,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
