namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched access badge target is in its life cycle.</summary>
public enum BatchedAccessBadgeTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched access badge target. Its code is unique within a tenant.</summary>
public sealed class BatchedAccessBadgeTarget(string code, string title, double hours, List<string> tags, string country, Dictionary<string, string> attributes, bool archived, int capacity, BatchedAccessBadgeTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public BatchedAccessBadgeTargetStatus Status { get; set; } = status;

    /// <summary>Starts a batched access badge target in status idle.</summary>
    public static BatchedAccessBadgeTarget NewBatchedAccessBadgeTarget(string code, string title)
    {
        return new BatchedAccessBadgeTarget(
            code,
            title,
            0.0,
            new List<string>(),
            "",
            new Dictionary<string, string>(),
            false,
            0,
            BatchedAccessBadgeTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAccessBadgeTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAccessBadgeTargetStatus next)
    {
        switch (Status)
        {
            case BatchedAccessBadgeTargetStatus.Idle:
                return next == BatchedAccessBadgeTargetStatus.Draining;
            case BatchedAccessBadgeTargetStatus.Warming:
                switch (next)
                {
                    case BatchedAccessBadgeTargetStatus.Draining:
                        return true;
                }
                return false;
            case BatchedAccessBadgeTargetStatus.Ready:
                return next == BatchedAccessBadgeTargetStatus.Stopped;
            case BatchedAccessBadgeTargetStatus.Draining:
                return next == BatchedAccessBadgeTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedAccessBadgeTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched access badge target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
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

    public double AdjustedTotal()
    {
        return Math.Round(Hours + (double)Capacity, 2);
    }

    public BatchedAccessBadgeTarget Copy()
    {
        return new BatchedAccessBadgeTarget(
            Code,
            Title,
            Hours,
            Tags,
            Country,
            Attributes,
            Archived,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
