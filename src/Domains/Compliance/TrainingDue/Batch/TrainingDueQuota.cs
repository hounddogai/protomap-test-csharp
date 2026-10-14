namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due quota is in its life cycle.</summary>
public enum TrainingDueQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A training due quota. Its code is unique within a tenant.</summary>
public sealed class TrainingDueQuota(string code, string label, string description, double minimum, double budget, int tenureMonths, string skill, string channel, int createdAt, TrainingDueQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Minimum { get; set; } = minimum;
    public double Budget { get; set; } = budget;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public string Channel { get; set; } = channel;
    public int CreatedAt { get; set; } = createdAt;
    public TrainingDueQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a training due quota in status proposed.</summary>
    public static TrainingDueQuota NewTrainingDueQuota(string code, string label)
    {
        return new TrainingDueQuota(code, label, "", 0.0, 0.0, 0, "", "", 0, TrainingDueQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueQuotaStatus next)
    {
        switch (Status)
        {
            case TrainingDueQuotaStatus.Proposed:
                switch (next)
                {
                    case TrainingDueQuotaStatus.Approved:
                        return true;
                }
                return false;
            case TrainingDueQuotaStatus.Approved:
                return next == TrainingDueQuotaStatus.Rejected || next == TrainingDueQuotaStatus.Withdrawn;
            case TrainingDueQuotaStatus.Rejected:
                switch (next)
                {
                    case TrainingDueQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)TenureMonths + (double)CreatedAt, 2);
    }

    public TrainingDueQuota Copy()
    {
        return new TrainingDueQuota(
            Code,
            Label,
            Description,
            Minimum,
            Budget,
            TenureMonths,
            Skill,
            Channel,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
