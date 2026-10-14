namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict nomination tier is in its life cycle.</summary>
public enum StrictNominationTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One strict nomination tier, with what the planners need to act on it.</summary>
public sealed class StrictNominationTier(string code, string title, string timezone, string employeeId, string badge, Dictionary<string, string> attributes, List<string> tags, int batchSize, StrictNominationTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string EmployeeId { get; set; } = employeeId;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public StrictNominationTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict nomination tier in status new.</summary>
    public static StrictNominationTier NewStrictNominationTier(string code, string title)
    {
        return new StrictNominationTier(
            code,
            title,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            new List<string>(),
            0,
            StrictNominationTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictNominationTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictNominationTierStatus next)
    {
        switch (Status)
        {
            case StrictNominationTierStatus.New:
                switch (next)
                {
                    case StrictNominationTierStatus.Running:
                    case StrictNominationTierStatus.Failed:
                        return true;
                }
                return false;
            case StrictNominationTierStatus.Queued:
                return next == StrictNominationTierStatus.Done;
            case StrictNominationTierStatus.Running:
                return next == StrictNominationTierStatus.Failed;
            case StrictNominationTierStatus.Done:
                return next == StrictNominationTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StrictNominationTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict nomination tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict nomination tier"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public StrictNominationTier Copy()
    {
        return new StrictNominationTier(
            Code,
            Title,
            Timezone,
            EmployeeId,
            Badge,
            Attributes,
            Tags,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
