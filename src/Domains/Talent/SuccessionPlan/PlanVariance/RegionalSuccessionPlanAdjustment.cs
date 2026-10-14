namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional succession plan adjustment is in its life cycle.</summary>
public enum RegionalSuccessionPlanAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One regional succession plan adjustment, with what the planners need to act on it.</summary>
public sealed class RegionalSuccessionPlanAdjustment(string code, string name, List<string> tags, string channel, int batchSize, string contactEmail, int retries, double amount, RegionalSuccessionPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public string ContactEmail { get; set; } = contactEmail;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public RegionalSuccessionPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional succession plan adjustment in status new.</summary>
    public static RegionalSuccessionPlanAdjustment NewRegionalSuccessionPlanAdjustment(string code, string name)
    {
        return new RegionalSuccessionPlanAdjustment(
            code,
            name,
            new List<string>(),
            "",
            0,
            "",
            0,
            0.0,
            RegionalSuccessionPlanAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSuccessionPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case RegionalSuccessionPlanAdjustmentStatus.New:
                switch (next)
                {
                    case RegionalSuccessionPlanAdjustmentStatus.Queued:
                    case RegionalSuccessionPlanAdjustmentStatus.Done:
                        return true;
                }
                return false;
            case RegionalSuccessionPlanAdjustmentStatus.Queued:
                return next == RegionalSuccessionPlanAdjustmentStatus.Failed;
            case RegionalSuccessionPlanAdjustmentStatus.Running:
                return next == RegionalSuccessionPlanAdjustmentStatus.Failed;
            case RegionalSuccessionPlanAdjustmentStatus.Done:
                switch (next)
                {
                    case RegionalSuccessionPlanAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalSuccessionPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional succession plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional succession plan adjustment"} {Code}";
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

    public double NetValue()
    {
        return Math.Round((double)Retries + Amount, 2);
    }

    public RegionalSuccessionPlanAdjustment Copy()
    {
        return new RegionalSuccessionPlanAdjustment(
            Code,
            Name,
            Tags,
            Channel,
            BatchSize,
            ContactEmail,
            Retries,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
