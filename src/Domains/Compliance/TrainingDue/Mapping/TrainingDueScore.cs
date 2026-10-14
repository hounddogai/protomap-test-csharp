namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due score is in its life cycle.</summary>
public enum TrainingDueScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due score as the training due team keeps it.</summary>
public sealed class TrainingDueScore(string code, string name, int leaveDays, int endsAt, int capacity, TrainingDueScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public TrainingDueScoreStatus Status { get; set; } = status;

    /// <summary>Starts a training due score in status open.</summary>
    public static TrainingDueScore NewTrainingDueScore(string code, string name)
    {
        return new TrainingDueScore(code, name, 0, 0, 0, TrainingDueScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueScoreStatus next)
    {
        switch (Status)
        {
            case TrainingDueScoreStatus.Open:
                return next == TrainingDueScoreStatus.Resolved;
            case TrainingDueScoreStatus.InReview:
                return next == TrainingDueScoreStatus.Resolved || next == TrainingDueScoreStatus.Reopened;
            case TrainingDueScoreStatus.Resolved:
                return next == TrainingDueScoreStatus.Reopened;
            case TrainingDueScoreStatus.Reopened:
                switch (next)
                {
                    case TrainingDueScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)EndsAt - (double)Capacity, 2);
    }

    public TrainingDueScore Copy()
    {
        return new TrainingDueScore(Code, Name, LeaveDays, EndsAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
