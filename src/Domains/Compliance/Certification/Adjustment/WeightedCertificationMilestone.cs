namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted certification milestone is in its life cycle.</summary>
public enum WeightedCertificationMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted certification milestone. Its code is unique within a tenant.</summary>
public sealed class WeightedCertificationMilestone(string code, string name, bool visible, string phone, double rating, double ratio, int quantity, bool approved, int updatedAt, string currency, WeightedCertificationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public WeightedCertificationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a weighted certification milestone in status proposed.</summary>
    public static WeightedCertificationMilestone NewWeightedCertificationMilestone(string code, string name)
    {
        return new WeightedCertificationMilestone(
            code,
            name,
            false,
            "",
            0.0,
            0.0,
            0,
            false,
            0,
            "",
            WeightedCertificationMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCertificationMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCertificationMilestoneStatus next)
    {
        switch (Status)
        {
            case WeightedCertificationMilestoneStatus.Proposed:
                switch (next)
                {
                    case WeightedCertificationMilestoneStatus.Approved:
                    case WeightedCertificationMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedCertificationMilestoneStatus.Approved:
                switch (next)
                {
                    case WeightedCertificationMilestoneStatus.Rejected:
                    case WeightedCertificationMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedCertificationMilestoneStatus.Rejected:
                switch (next)
                {
                    case WeightedCertificationMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedCertificationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted certification milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted certification milestone"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)UpdatedAt + (double)Quantity, 2);
    }

    public WeightedCertificationMilestone Copy()
    {
        return new WeightedCertificationMilestone(
            Code,
            Name,
            Visible,
            Phone,
            Rating,
            Ratio,
            Quantity,
            Approved,
            UpdatedAt,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
