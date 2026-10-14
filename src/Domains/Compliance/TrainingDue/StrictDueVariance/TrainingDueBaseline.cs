namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due baseline is in its life cycle.</summary>
public enum TrainingDueBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A training due baseline. Its code is unique within a tenant.</summary>
public sealed class TrainingDueBaseline(string code, string label, string reviewer, int startsAt, string grade, int priority, string badge, int batchSize, int expiresAt, TrainingDueBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public TrainingDueBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a training due baseline in status proposed.</summary>
    public static TrainingDueBaseline NewTrainingDueBaseline(string code, string label)
    {
        return new TrainingDueBaseline(code, label, "", 0, "", 0, "", 0, 0, TrainingDueBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueBaselineStatus next)
    {
        switch (Status)
        {
            case TrainingDueBaselineStatus.Proposed:
                switch (next)
                {
                    case TrainingDueBaselineStatus.Rejected:
                    case TrainingDueBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case TrainingDueBaselineStatus.Approved:
                switch (next)
                {
                    case TrainingDueBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case TrainingDueBaselineStatus.Rejected:
                switch (next)
                {
                    case TrainingDueBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize - (double)ExpiresAt, 2);
    }

    public TrainingDueBaseline Copy()
    {
        return new TrainingDueBaseline(
            Code,
            Label,
            Reviewer,
            StartsAt,
            Grade,
            Priority,
            Badge,
            BatchSize,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
