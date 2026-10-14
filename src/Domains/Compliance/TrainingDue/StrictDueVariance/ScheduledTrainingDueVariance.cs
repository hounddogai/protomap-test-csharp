namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled training due variance is in its life cycle.</summary>
public enum ScheduledTrainingDueVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled training due variance. Its code is unique within a tenant.</summary>
public sealed class ScheduledTrainingDueVariance(string code, string name, int createdAt, int startsAt, string category, string externalRef, int endsAt, bool active, int attempts, int updatedAt, bool pinned, ScheduledTrainingDueVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public bool Active { get; set; } = active;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Pinned { get; set; } = pinned;
    public ScheduledTrainingDueVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled training due variance in status draft.</summary>
    public static ScheduledTrainingDueVariance NewScheduledTrainingDueVariance(string code, string name)
    {
        return new ScheduledTrainingDueVariance(
            code,
            name,
            0,
            0,
            "",
            "",
            0,
            false,
            0,
            0,
            false,
            ScheduledTrainingDueVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTrainingDueVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTrainingDueVarianceStatus next)
    {
        switch (Status)
        {
            case ScheduledTrainingDueVarianceStatus.Draft:
                return next == ScheduledTrainingDueVarianceStatus.Pending || next == ScheduledTrainingDueVarianceStatus.Closed;
            case ScheduledTrainingDueVarianceStatus.Pending:
                return next == ScheduledTrainingDueVarianceStatus.Closed;
            case ScheduledTrainingDueVarianceStatus.Active:
                return next == ScheduledTrainingDueVarianceStatus.Suspended;
            case ScheduledTrainingDueVarianceStatus.Suspended:
                switch (next)
                {
                    case ScheduledTrainingDueVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTrainingDueVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled training due variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt + (double)CreatedAt, 2);
    }

    public ScheduledTrainingDueVariance Copy()
    {
        return new ScheduledTrainingDueVariance(
            Code,
            Name,
            CreatedAt,
            StartsAt,
            Category,
            ExternalRef,
            EndsAt,
            Active,
            Attempts,
            UpdatedAt,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
