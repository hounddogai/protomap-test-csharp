namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global span adjustment is in its life cycle.</summary>
public enum GlobalSpanAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global span adjustment. Its code is unique within a tenant.</summary>
public sealed class GlobalSpanAdjustment(string code, string title, List<string> tags, double floorValue, bool approved, int sequence, string description, string label, GlobalSpanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public bool Approved { get; set; } = approved;
    public int Sequence { get; set; } = sequence;
    public string Description { get; set; } = description;
    public string Label { get; set; } = label;
    public GlobalSpanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a global span adjustment in status planned.</summary>
    public static GlobalSpanAdjustment NewGlobalSpanAdjustment(string code, string title)
    {
        return new GlobalSpanAdjustment(
            code,
            title,
            new List<string>(),
            0.0,
            false,
            0,
            "",
            "",
            GlobalSpanAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSpanAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSpanAdjustmentStatus next)
    {
        switch (Status)
        {
            case GlobalSpanAdjustmentStatus.Planned:
                switch (next)
                {
                    case GlobalSpanAdjustmentStatus.InProgress:
                    case GlobalSpanAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case GlobalSpanAdjustmentStatus.Scheduled:
                switch (next)
                {
                    case GlobalSpanAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case GlobalSpanAdjustmentStatus.InProgress:
                switch (next)
                {
                    case GlobalSpanAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case GlobalSpanAdjustmentStatus.Completed:
                return next == GlobalSpanAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalSpanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global span adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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
        return Math.Round((double)Sequence + FloorValue, 2);
    }

    public GlobalSpanAdjustment Copy()
    {
        return new GlobalSpanAdjustment(
            Code,
            Title,
            Tags,
            FloorValue,
            Approved,
            Sequence,
            Description,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
