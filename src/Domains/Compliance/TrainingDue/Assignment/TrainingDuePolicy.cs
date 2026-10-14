namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due policy is in its life cycle.</summary>
public enum TrainingDuePolicyStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a training due policy and its current status.</summary>
public sealed class TrainingDuePolicy(string code, string name, string channel, double rating, string skill, string salaryBand, string description, double threshold, string country, int version, string managerId, TrainingDuePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Rating { get; set; } = rating;
    public string Skill { get; set; } = skill;
    public string SalaryBand { get; set; } = salaryBand;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public string ManagerId { get; set; } = managerId;
    public TrainingDuePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a training due policy in status requested.</summary>
    public static TrainingDuePolicy NewTrainingDuePolicy(string code, string name)
    {
        return new TrainingDuePolicy(
            code,
            name,
            "",
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            0,
            "",
            TrainingDuePolicyStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDuePolicyStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDuePolicyStatus next)
    {
        switch (Status)
        {
            case TrainingDuePolicyStatus.Requested:
                switch (next)
                {
                    case TrainingDuePolicyStatus.Granted:
                    case TrainingDuePolicyStatus.Expired:
                        return true;
                }
                return false;
            case TrainingDuePolicyStatus.Granted:
                return next == TrainingDuePolicyStatus.Revoked || next == TrainingDuePolicyStatus.Expired;
            case TrainingDuePolicyStatus.Revoked:
                return next == TrainingDuePolicyStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TrainingDuePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Rating * Threshold, 2);
    }

    public TrainingDuePolicy Copy()
    {
        return new TrainingDuePolicy(
            Code,
            Name,
            Channel,
            Rating,
            Skill,
            SalaryBand,
            Description,
            Threshold,
            Country,
            Version,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
