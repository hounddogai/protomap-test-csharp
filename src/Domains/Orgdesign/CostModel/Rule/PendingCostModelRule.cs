namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending cost model rule is in its life cycle.</summary>
public enum PendingCostModelRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending cost model rule. Its code is unique within a tenant.</summary>
public sealed class PendingCostModelRule(string code, string label, string summary, bool pinned, double cost, double hours, double rating, string department, int priority, PendingCostModelRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public double Rating { get; set; } = rating;
    public string Department { get; set; } = department;
    public int Priority { get; set; } = priority;
    public PendingCostModelRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pending cost model rule in status new.</summary>
    public static PendingCostModelRule NewPendingCostModelRule(string code, string label)
    {
        return new PendingCostModelRule(
            code,
            label,
            "",
            false,
            0.0,
            0.0,
            0.0,
            "",
            0,
            PendingCostModelRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingCostModelRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingCostModelRuleStatus next)
    {
        switch (Status)
        {
            case PendingCostModelRuleStatus.New:
                return next == PendingCostModelRuleStatus.Failed;
            case PendingCostModelRuleStatus.Queued:
                return next == PendingCostModelRuleStatus.Running;
            case PendingCostModelRuleStatus.Running:
                switch (next)
                {
                    case PendingCostModelRuleStatus.Failed:
                        return true;
                }
                return false;
            case PendingCostModelRuleStatus.Done:
                switch (next)
                {
                    case PendingCostModelRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingCostModelRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending cost model rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round(Rating + Hours, 2);
    }

    public PendingCostModelRule Copy()
    {
        return new PendingCostModelRule(
            Code,
            Label,
            Summary,
            Pinned,
            Cost,
            Hours,
            Rating,
            Department,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
