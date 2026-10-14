namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled recognition bundle is in its life cycle.</summary>
public enum ScheduledRecognitionBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scheduled recognition bundle and its current status.</summary>
public sealed class ScheduledRecognitionBundle(string code, string label, double hours, string grade, int quantity, double ceiling, int revision, bool pinned, int tenureMonths, ScheduledRecognitionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledRecognitionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled recognition bundle in status open.</summary>
    public static ScheduledRecognitionBundle NewScheduledRecognitionBundle(string code, string label)
    {
        return new ScheduledRecognitionBundle(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            0,
            false,
            0,
            ScheduledRecognitionBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRecognitionBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRecognitionBundleStatus next)
    {
        switch (Status)
        {
            case ScheduledRecognitionBundleStatus.Open:
                return next == ScheduledRecognitionBundleStatus.Resolved || next == ScheduledRecognitionBundleStatus.Archived;
            case ScheduledRecognitionBundleStatus.InReview:
                switch (next)
                {
                    case ScheduledRecognitionBundleStatus.Resolved:
                    case ScheduledRecognitionBundleStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledRecognitionBundleStatus.Resolved:
                switch (next)
                {
                    case ScheduledRecognitionBundleStatus.Reopened:
                    case ScheduledRecognitionBundleStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledRecognitionBundleStatus.Reopened:
                switch (next)
                {
                    case ScheduledRecognitionBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledRecognitionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled recognition bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling * (double)Quantity, 2);
    }

    public ScheduledRecognitionBundle Copy()
    {
        return new ScheduledRecognitionBundle(
            Code,
            Label,
            Hours,
            Grade,
            Quantity,
            Ceiling,
            Revision,
            Pinned,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
