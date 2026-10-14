namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted merit cycle segment is in its life cycle.</summary>
public enum WeightedMeritCycleSegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A weighted merit cycle segment. Its code is unique within a tenant.</summary>
public sealed class WeightedMeritCycleSegment(string code, string title, double threshold, double floorValue, string region, int windowDays, Dictionary<string, string> attributes, int batchSize, string ownerId, string phone, WeightedMeritCycleSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public string Region { get; set; } = region;
    public int WindowDays { get; set; } = windowDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public WeightedMeritCycleSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a weighted merit cycle segment in status open.</summary>
    public static WeightedMeritCycleSegment NewWeightedMeritCycleSegment(string code, string title)
    {
        return new WeightedMeritCycleSegment(
            code,
            title,
            0.0,
            0.0,
            "",
            0,
            new Dictionary<string, string>(),
            0,
            "",
            "",
            WeightedMeritCycleSegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMeritCycleSegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMeritCycleSegmentStatus next)
    {
        switch (Status)
        {
            case WeightedMeritCycleSegmentStatus.Open:
                return next == WeightedMeritCycleSegmentStatus.Reopened;
            case WeightedMeritCycleSegmentStatus.InReview:
                return next == WeightedMeritCycleSegmentStatus.Resolved || next == WeightedMeritCycleSegmentStatus.Archived;
            case WeightedMeritCycleSegmentStatus.Resolved:
                switch (next)
                {
                    case WeightedMeritCycleSegmentStatus.Reopened:
                        return true;
                }
                return false;
            case WeightedMeritCycleSegmentStatus.Reopened:
                return next == WeightedMeritCycleSegmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedMeritCycleSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted merit cycle segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedValue()
    {
        return Math.Round((double)WindowDays - FloorValue, 2);
    }

    public WeightedMeritCycleSegment Copy()
    {
        return new WeightedMeritCycleSegment(
            Code,
            Title,
            Threshold,
            FloorValue,
            Region,
            WindowDays,
            Attributes,
            BatchSize,
            OwnerId,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
