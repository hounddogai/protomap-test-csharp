namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted cost model policy is in its life cycle.</summary>
public enum WeightedCostModelPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted cost model policy as the cost model team keeps it.</summary>
public sealed class WeightedCostModelPolicy(string code, string title, string segment, double hours, string description, List<string> tags, string notes, int attempts, WeightedCostModelPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public WeightedCostModelPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a weighted cost model policy in status new.</summary>
    public static WeightedCostModelPolicy NewWeightedCostModelPolicy(string code, string title)
    {
        return new WeightedCostModelPolicy(
            code,
            title,
            "",
            0.0,
            "",
            new List<string>(),
            "",
            0,
            WeightedCostModelPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCostModelPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCostModelPolicyStatus next)
    {
        switch (Status)
        {
            case WeightedCostModelPolicyStatus.New:
                return next == WeightedCostModelPolicyStatus.Queued || next == WeightedCostModelPolicyStatus.Done;
            case WeightedCostModelPolicyStatus.Queued:
                switch (next)
                {
                    case WeightedCostModelPolicyStatus.Done:
                        return true;
                }
                return false;
            case WeightedCostModelPolicyStatus.Running:
                return next == WeightedCostModelPolicyStatus.Done;
            case WeightedCostModelPolicyStatus.Done:
                switch (next)
                {
                    case WeightedCostModelPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedCostModelPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted cost model policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double AdjustedTotal()
    {
        return Math.Round(Hours * (double)Attempts, 2);
    }

    public WeightedCostModelPolicy Copy()
    {
        return new WeightedCostModelPolicy(Code, Title, Segment, Hours, Description, Tags, Notes, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
