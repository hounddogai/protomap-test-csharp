namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh record is in its life cycle.</summary>
public enum RefreshRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh record as the refresh team keeps it.</summary>
public sealed class RefreshRecord(string code, string label, double rate, bool active, int expiresAt, string grade, double threshold, string segment, RefreshRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public double Threshold { get; set; } = threshold;
    public string Segment { get; set; } = segment;
    public RefreshRecordStatus Status { get; set; } = status;

    /// <summary>Starts a refresh record in status planned.</summary>
    public static RefreshRecord NewRefreshRecord(string code, string label)
    {
        return new RefreshRecord(code, label, 0.0, false, 0, "", 0.0, "", RefreshRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshRecordStatus next)
    {
        switch (Status)
        {
            case RefreshRecordStatus.Planned:
                return next == RefreshRecordStatus.InProgress || next == RefreshRecordStatus.Completed;
            case RefreshRecordStatus.Scheduled:
                return next == RefreshRecordStatus.Completed || next == RefreshRecordStatus.Cancelled;
            case RefreshRecordStatus.InProgress:
                switch (next)
                {
                    case RefreshRecordStatus.Completed:
                    case RefreshRecordStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshRecordStatus.Completed:
                return next == RefreshRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RefreshRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Threshold * (double)ExpiresAt, 2);
    }

    public RefreshRecord Copy()
    {
        return new RefreshRecord(Code, Label, Rate, Active, ExpiresAt, Grade, Threshold, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
