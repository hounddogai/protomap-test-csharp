namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback nomination target is in its life cycle.</summary>
public enum FallbackNominationTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback nomination target as the nomination team keeps it.</summary>
public sealed class FallbackNominationTarget(string code, string title, double ratio, string skill, int priority, double fte, double margin, int createdAt, Dictionary<string, string> attributes, FallbackNominationTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public int CreatedAt { get; set; } = createdAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackNominationTargetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback nomination target in status new.</summary>
    public static FallbackNominationTarget NewFallbackNominationTarget(string code, string title)
    {
        return new FallbackNominationTarget(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            0.0,
            0,
            new Dictionary<string, string>(),
            FallbackNominationTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackNominationTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackNominationTargetStatus next)
    {
        switch (Status)
        {
            case FallbackNominationTargetStatus.New:
                return next == FallbackNominationTargetStatus.Failed;
            case FallbackNominationTargetStatus.Queued:
                switch (next)
                {
                    case FallbackNominationTargetStatus.Running:
                    case FallbackNominationTargetStatus.Failed:
                        return true;
                }
                return false;
            case FallbackNominationTargetStatus.Running:
                switch (next)
                {
                    case FallbackNominationTargetStatus.Failed:
                        return true;
                }
                return false;
            case FallbackNominationTargetStatus.Done:
                switch (next)
                {
                    case FallbackNominationTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackNominationTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback nomination target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback nomination target"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round(Ratio * (double)CreatedAt, 2);
    }

    public FallbackNominationTarget Copy()
    {
        return new FallbackNominationTarget(
            Code,
            Title,
            Ratio,
            Skill,
            Priority,
            Fte,
            Margin,
            CreatedAt,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
