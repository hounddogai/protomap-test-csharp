namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay run milestone is in its life cycle.</summary>
public enum TieredPayRunMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tiered pay run milestone and its current status.</summary>
public sealed class TieredPayRunMilestone(string code, string label, int tenureMonths, Dictionary<string, string> attributes, double rate, double threshold, int attempts, int capacity, bool visible, TieredPayRunMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public bool Visible { get; set; } = visible;
    public TieredPayRunMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay run milestone in status idle.</summary>
    public static TieredPayRunMilestone NewTieredPayRunMilestone(string code, string label)
    {
        return new TieredPayRunMilestone(
            code,
            label,
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0,
            0,
            false,
            TieredPayRunMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayRunMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayRunMilestoneStatus next)
    {
        switch (Status)
        {
            case TieredPayRunMilestoneStatus.Idle:
                return next == TieredPayRunMilestoneStatus.Warming || next == TieredPayRunMilestoneStatus.Draining;
            case TieredPayRunMilestoneStatus.Warming:
                switch (next)
                {
                    case TieredPayRunMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPayRunMilestoneStatus.Ready:
                switch (next)
                {
                    case TieredPayRunMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPayRunMilestoneStatus.Draining:
                return next == TieredPayRunMilestoneStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredPayRunMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay run milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double NetVolume()
    {
        return Math.Round(Rate + (double)Attempts, 2);
    }

    public TieredPayRunMilestone Copy()
    {
        return new TieredPayRunMilestone(
            Code,
            Label,
            TenureMonths,
            Attributes,
            Rate,
            Threshold,
            Attempts,
            Capacity,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
