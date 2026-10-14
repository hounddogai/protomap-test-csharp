namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due schedule is in its life cycle.</summary>
public enum TrainingDueScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due schedule as the training due team keeps it.</summary>
public sealed class TrainingDueSchedule(string code, string title, string salaryBand, int sequence, double score, int createdAt, string category, bool approved, TrainingDueScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public TrainingDueScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a training due schedule in status open.</summary>
    public static TrainingDueSchedule NewTrainingDueSchedule(string code, string title)
    {
        return new TrainingDueSchedule(code, title, "", 0, 0.0, 0, "", false, TrainingDueScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueScheduleStatus next)
    {
        switch (Status)
        {
            case TrainingDueScheduleStatus.Open:
                return next == TrainingDueScheduleStatus.Resolved;
            case TrainingDueScheduleStatus.InReview:
                switch (next)
                {
                    case TrainingDueScheduleStatus.Resolved:
                    case TrainingDueScheduleStatus.Archived:
                        return true;
                }
                return false;
            case TrainingDueScheduleStatus.Resolved:
                switch (next)
                {
                    case TrainingDueScheduleStatus.Reopened:
                    case TrainingDueScheduleStatus.Archived:
                        return true;
                }
                return false;
            case TrainingDueScheduleStatus.Reopened:
                switch (next)
                {
                    case TrainingDueScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"training due schedule"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Score - (double)CreatedAt, 2);
    }

    public TrainingDueSchedule Copy()
    {
        return new TrainingDueSchedule(
            Code,
            Title,
            SalaryBand,
            Sequence,
            Score,
            CreatedAt,
            Category,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
