namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled reorg threshold is in its life cycle.</summary>
public enum ScheduledReorgThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled reorg threshold as the reorg team keeps it.</summary>
public sealed class ScheduledReorgThreshold(string code, string label, int endsAt, int quantity, string title, ScheduledReorgThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int Quantity { get; set; } = quantity;
    public string Title { get; set; } = title;
    public ScheduledReorgThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled reorg threshold in status open.</summary>
    public static ScheduledReorgThreshold NewScheduledReorgThreshold(string code, string label)
    {
        return new ScheduledReorgThreshold(code, label, 0, 0, "", ScheduledReorgThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledReorgThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledReorgThresholdStatus next)
    {
        switch (Status)
        {
            case ScheduledReorgThresholdStatus.Open:
                return next == ScheduledReorgThresholdStatus.Resolved;
            case ScheduledReorgThresholdStatus.InReview:
                switch (next)
                {
                    case ScheduledReorgThresholdStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledReorgThresholdStatus.Resolved:
                return next == ScheduledReorgThresholdStatus.Archived;
            case ScheduledReorgThresholdStatus.Reopened:
                switch (next)
                {
                    case ScheduledReorgThresholdStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledReorgThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled reorg threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Quantity + (double)EndsAt, 2);
    }

    public ScheduledReorgThreshold Copy()
    {
        return new ScheduledReorgThreshold(Code, Label, EndsAt, Quantity, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
