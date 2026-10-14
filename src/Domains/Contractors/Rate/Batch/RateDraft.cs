namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate draft is in its life cycle.</summary>
public enum RateDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rate draft. Its code is unique within a tenant.</summary>
public sealed class RateDraft(string code, string name, string title, bool active, string category, string department, int capacity, Dictionary<string, string> attributes, string badge, string region, int version, RateDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Category { get; set; } = category;
    public string Department { get; set; } = department;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public string Region { get; set; } = region;
    public int Version { get; set; } = version;
    public RateDraftStatus Status { get; set; } = status;

    /// <summary>Starts a rate draft in status open.</summary>
    public static RateDraft NewRateDraft(string code, string name)
    {
        return new RateDraft(
            code,
            name,
            "",
            false,
            "",
            "",
            0,
            new Dictionary<string, string>(),
            "",
            "",
            0,
            RateDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateDraftStatus next)
    {
        switch (Status)
        {
            case RateDraftStatus.Open:
                switch (next)
                {
                    case RateDraftStatus.Archived:
                        return true;
                }
                return false;
            case RateDraftStatus.InReview:
                switch (next)
                {
                    case RateDraftStatus.Resolved:
                    case RateDraftStatus.Archived:
                        return true;
                }
                return false;
            case RateDraftStatus.Resolved:
                return next == RateDraftStatus.Reopened;
            case RateDraftStatus.Reopened:
                switch (next)
                {
                    case RateDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"rate draft"} {Code}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Version - (double)Capacity, 2);
    }

    public RateDraft Copy()
    {
        return new RateDraft(
            Code,
            Name,
            Title,
            Active,
            Category,
            Department,
            Capacity,
            Attributes,
            Badge,
            Region,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
