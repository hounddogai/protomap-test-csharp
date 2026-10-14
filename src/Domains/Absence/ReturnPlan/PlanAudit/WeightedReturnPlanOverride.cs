namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted return plan override is in its life cycle.</summary>
public enum WeightedReturnPlanOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted return plan override and its current status.</summary>
public sealed class WeightedReturnPlanOverride(string code, string label, double score, List<string> tags, string jobCode, int windowDays, int revision, string category, string ownerId, string timezone, WeightedReturnPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public List<string> Tags { get; set; } = tags;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public int Revision { get; set; } = revision;
    public string Category { get; set; } = category;
    public string OwnerId { get; set; } = ownerId;
    public string Timezone { get; set; } = timezone;
    public WeightedReturnPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a weighted return plan override in status proposed.</summary>
    public static WeightedReturnPlanOverride NewWeightedReturnPlanOverride(string code, string label)
    {
        return new WeightedReturnPlanOverride(
            code,
            label,
            0.0,
            new List<string>(),
            "",
            0,
            0,
            "",
            "",
            "",
            WeightedReturnPlanOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReturnPlanOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReturnPlanOverrideStatus next)
    {
        switch (Status)
        {
            case WeightedReturnPlanOverrideStatus.Proposed:
                switch (next)
                {
                    case WeightedReturnPlanOverrideStatus.Approved:
                    case WeightedReturnPlanOverrideStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedReturnPlanOverrideStatus.Approved:
                return next == WeightedReturnPlanOverrideStatus.Rejected || next == WeightedReturnPlanOverrideStatus.Withdrawn;
            case WeightedReturnPlanOverrideStatus.Rejected:
                switch (next)
                {
                    case WeightedReturnPlanOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReturnPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted return plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)WindowDays * Score, 2);
    }

    public WeightedReturnPlanOverride Copy()
    {
        return new WeightedReturnPlanOverride(
            Code,
            Label,
            Score,
            Tags,
            JobCode,
            WindowDays,
            Revision,
            Category,
            OwnerId,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
