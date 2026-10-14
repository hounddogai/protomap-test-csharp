namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due profile is in its life cycle.</summary>
public enum TrainingDueProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A training due profile as the training due team keeps it.</summary>
public sealed class TrainingDueProfile(string code, string label, string jobCode, int createdAt, int tenureMonths, string costCenter, double minimum, int retries, int expiresAt, List<string> tags, TrainingDueProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string CostCenter { get; set; } = costCenter;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public int ExpiresAt { get; set; } = expiresAt;
    public List<string> Tags { get; set; } = tags;
    public TrainingDueProfileStatus Status { get; set; } = status;

    /// <summary>Starts a training due profile in status draft.</summary>
    public static TrainingDueProfile NewTrainingDueProfile(string code, string label)
    {
        return new TrainingDueProfile(
            code,
            label,
            "",
            0,
            0,
            "",
            0.0,
            0,
            0,
            new List<string>(),
            TrainingDueProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueProfileStatus next)
    {
        switch (Status)
        {
            case TrainingDueProfileStatus.Draft:
                switch (next)
                {
                    case TrainingDueProfileStatus.Active:
                    case TrainingDueProfileStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueProfileStatus.Pending:
                return next == TrainingDueProfileStatus.Closed;
            case TrainingDueProfileStatus.Active:
                switch (next)
                {
                    case TrainingDueProfileStatus.Closed:
                        return true;
                }
                return false;
            case TrainingDueProfileStatus.Suspended:
                return next == TrainingDueProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDueProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double AdjustedExposure()
    {
        return Math.Round(Minimum * (double)CreatedAt, 2);
    }

    public TrainingDueProfile Copy()
    {
        return new TrainingDueProfile(
            Code,
            Label,
            JobCode,
            CreatedAt,
            TenureMonths,
            CostCenter,
            Minimum,
            Retries,
            ExpiresAt,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
