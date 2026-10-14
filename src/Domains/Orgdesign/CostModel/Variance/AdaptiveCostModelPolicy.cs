namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive cost model policy is in its life cycle.</summary>
public enum AdaptiveCostModelPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive cost model policy. Its code is unique within a tenant.</summary>
public sealed class AdaptiveCostModelPolicy(string code, string label, bool archived, string externalRef, string location, int sequence, string title, string skill, double maximum, AdaptiveCostModelPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public double Maximum { get; set; } = maximum;
    public AdaptiveCostModelPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive cost model policy in status idle.</summary>
    public static AdaptiveCostModelPolicy NewAdaptiveCostModelPolicy(string code, string label)
    {
        return new AdaptiveCostModelPolicy(
            code,
            label,
            false,
            "",
            "",
            0,
            "",
            "",
            0.0,
            AdaptiveCostModelPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCostModelPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCostModelPolicyStatus next)
    {
        switch (Status)
        {
            case AdaptiveCostModelPolicyStatus.Idle:
                switch (next)
                {
                    case AdaptiveCostModelPolicyStatus.Warming:
                    case AdaptiveCostModelPolicyStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveCostModelPolicyStatus.Warming:
                switch (next)
                {
                    case AdaptiveCostModelPolicyStatus.Ready:
                        return true;
                }
                return false;
            case AdaptiveCostModelPolicyStatus.Ready:
                return next == AdaptiveCostModelPolicyStatus.Stopped;
            case AdaptiveCostModelPolicyStatus.Draining:
                switch (next)
                {
                    case AdaptiveCostModelPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveCostModelPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive cost model policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence + Maximum, 2);
    }

    public AdaptiveCostModelPolicy Copy()
    {
        return new AdaptiveCostModelPolicy(
            Code,
            Label,
            Archived,
            ExternalRef,
            Location,
            Sequence,
            Title,
            Skill,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
