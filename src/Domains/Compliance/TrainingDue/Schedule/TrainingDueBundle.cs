namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due bundle is in its life cycle.</summary>
public enum TrainingDueBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A training due bundle. Its code is unique within a tenant.</summary>
public sealed class TrainingDueBundle(string code, string title, int priority, string category, string timezone, string channel, int leaveDays, string ownerId, double score, string currency, TrainingDueBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string Category { get; set; } = category;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public TrainingDueBundleStatus Status { get; set; } = status;

    /// <summary>Starts a training due bundle in status idle.</summary>
    public static TrainingDueBundle NewTrainingDueBundle(string code, string title)
    {
        return new TrainingDueBundle(code, title, 0, "", "", "", 0, "", 0.0, "", TrainingDueBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueBundleStatus next)
    {
        switch (Status)
        {
            case TrainingDueBundleStatus.Idle:
                return next == TrainingDueBundleStatus.Ready || next == TrainingDueBundleStatus.Draining;
            case TrainingDueBundleStatus.Warming:
                switch (next)
                {
                    case TrainingDueBundleStatus.Stopped:
                        return true;
                }
                return false;
            case TrainingDueBundleStatus.Ready:
                return next == TrainingDueBundleStatus.Stopped;
            case TrainingDueBundleStatus.Draining:
                switch (next)
                {
                    case TrainingDueBundleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score - (double)LeaveDays, 2);
    }

    public TrainingDueBundle Copy()
    {
        return new TrainingDueBundle(
            Code,
            Title,
            Priority,
            Category,
            Timezone,
            Channel,
            LeaveDays,
            OwnerId,
            Score,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
