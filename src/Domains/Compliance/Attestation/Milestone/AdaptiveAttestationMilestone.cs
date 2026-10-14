namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive attestation milestone is in its life cycle.</summary>
public enum AdaptiveAttestationMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a adaptive attestation milestone and its current status.</summary>
public sealed class AdaptiveAttestationMilestone(string code, string name, string grade, string location, string region, string phone, double weight, double ceiling, double cost, AdaptiveAttestationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Location { get; set; } = location;
    public string Region { get; set; } = region;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public AdaptiveAttestationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive attestation milestone in status planned.</summary>
    public static AdaptiveAttestationMilestone NewAdaptiveAttestationMilestone(string code, string name)
    {
        return new AdaptiveAttestationMilestone(
            code,
            name,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            AdaptiveAttestationMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAttestationMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAttestationMilestoneStatus next)
    {
        switch (Status)
        {
            case AdaptiveAttestationMilestoneStatus.Planned:
                switch (next)
                {
                    case AdaptiveAttestationMilestoneStatus.Completed:
                        return true;
                }
                return false;
            case AdaptiveAttestationMilestoneStatus.Scheduled:
                return next == AdaptiveAttestationMilestoneStatus.Completed || next == AdaptiveAttestationMilestoneStatus.Cancelled;
            case AdaptiveAttestationMilestoneStatus.InProgress:
                switch (next)
                {
                    case AdaptiveAttestationMilestoneStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveAttestationMilestoneStatus.Completed:
                return next == AdaptiveAttestationMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveAttestationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive attestation milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Cost * Ceiling, 2);
    }

    public AdaptiveAttestationMilestone Copy()
    {
        return new AdaptiveAttestationMilestone(
            Code,
            Name,
            Grade,
            Location,
            Region,
            Phone,
            Weight,
            Ceiling,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
