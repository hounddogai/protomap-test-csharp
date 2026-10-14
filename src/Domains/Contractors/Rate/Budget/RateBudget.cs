namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate budget is in its life cycle.</summary>
public enum RateBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rate budget and its current status.</summary>
public sealed class RateBudget(string code, string name, double threshold, double minimum, bool active, Dictionary<string, string> attributes, double cost, int version, string ownerId, List<string> tags, int startsAt, RateBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public int Version { get; set; } = version;
    public string OwnerId { get; set; } = ownerId;
    public List<string> Tags { get; set; } = tags;
    public int StartsAt { get; set; } = startsAt;
    public RateBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a rate budget in status proposed.</summary>
    public static RateBudget NewRateBudget(string code, string name)
    {
        return new RateBudget(
            code,
            name,
            0.0,
            0.0,
            false,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            new List<string>(),
            0,
            RateBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateBudgetStatus next)
    {
        switch (Status)
        {
            case RateBudgetStatus.Proposed:
                return next == RateBudgetStatus.Approved;
            case RateBudgetStatus.Approved:
                switch (next)
                {
                    case RateBudgetStatus.Rejected:
                        return true;
                }
                return false;
            case RateBudgetStatus.Rejected:
                switch (next)
                {
                    case RateBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double EffectiveLoad()
    {
        return Math.Round(Minimum * (double)Version, 2);
    }

    public RateBudget Copy()
    {
        return new RateBudget(
            Code,
            Name,
            Threshold,
            Minimum,
            Active,
            Attributes,
            Cost,
            Version,
            OwnerId,
            Tags,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
