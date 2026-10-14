namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive warranty milestone is in its life cycle.</summary>
public enum AdaptiveWarrantyMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a adaptive warranty milestone and its current status.</summary>
public sealed class AdaptiveWarrantyMilestone(string code, string label, int limit, string checksum, string unit, double budget, string costCenter, List<string> tags, double weight, double amount, AdaptiveWarrantyMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public string Unit { get; set; } = unit;
    public double Budget { get; set; } = budget;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public double Weight { get; set; } = weight;
    public double Amount { get; set; } = amount;
    public AdaptiveWarrantyMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive warranty milestone in status proposed.</summary>
    public static AdaptiveWarrantyMilestone NewAdaptiveWarrantyMilestone(string code, string label)
    {
        return new AdaptiveWarrantyMilestone(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            new List<string>(),
            0.0,
            0.0,
            AdaptiveWarrantyMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveWarrantyMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveWarrantyMilestoneStatus next)
    {
        switch (Status)
        {
            case AdaptiveWarrantyMilestoneStatus.Proposed:
                switch (next)
                {
                    case AdaptiveWarrantyMilestoneStatus.Approved:
                    case AdaptiveWarrantyMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdaptiveWarrantyMilestoneStatus.Approved:
                switch (next)
                {
                    case AdaptiveWarrantyMilestoneStatus.Rejected:
                    case AdaptiveWarrantyMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdaptiveWarrantyMilestoneStatus.Rejected:
                return next == AdaptiveWarrantyMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveWarrantyMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive warranty milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit - Budget, 2);
    }

    public AdaptiveWarrantyMilestone Copy()
    {
        return new AdaptiveWarrantyMilestone(
            Code,
            Label,
            Limit,
            Checksum,
            Unit,
            Budget,
            CostCenter,
            Tags,
            Weight,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
