namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback snapshot is in its life cycle.</summary>
public enum FeedbackSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One feedback snapshot, with what the planners need to act on it.</summary>
public sealed class FeedbackSnapshot(string code, string label, string team, string department, double weight, double rating, double margin, double amount, int revision, double floorValue, string source, FeedbackSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public double Amount { get; set; } = amount;
    public int Revision { get; set; } = revision;
    public double FloorValue { get; set; } = floorValue;
    public string Source { get; set; } = source;
    public FeedbackSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a feedback snapshot in status proposed.</summary>
    public static FeedbackSnapshot NewFeedbackSnapshot(string code, string label)
    {
        return new FeedbackSnapshot(
            code,
            label,
            "",
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            0,
            0.0,
            "",
            FeedbackSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackSnapshotStatus next)
    {
        switch (Status)
        {
            case FeedbackSnapshotStatus.Proposed:
                return next == FeedbackSnapshotStatus.Approved;
            case FeedbackSnapshotStatus.Approved:
                switch (next)
                {
                    case FeedbackSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
            case FeedbackSnapshotStatus.Rejected:
                switch (next)
                {
                    case FeedbackSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)Revision + Weight, 2);
    }

    public FeedbackSnapshot Copy()
    {
        return new FeedbackSnapshot(
            Code,
            Label,
            Team,
            Department,
            Weight,
            Rating,
            Margin,
            Amount,
            Revision,
            FloorValue,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
