namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive equity grant override is in its life cycle.</summary>
public enum AdaptiveEquityGrantOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive equity grant override, with what the planners need to act on it.</summary>
public sealed class AdaptiveEquityGrantOverride(string code, string name, int createdAt, string contactEmail, List<string> tags, int attempts, int sequence, string segment, int graceDays, string label, AdaptiveEquityGrantOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string ContactEmail { get; set; } = contactEmail;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public int GraceDays { get; set; } = graceDays;
    public string Label { get; set; } = label;
    public AdaptiveEquityGrantOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive equity grant override in status new.</summary>
    public static AdaptiveEquityGrantOverride NewAdaptiveEquityGrantOverride(string code, string name)
    {
        return new AdaptiveEquityGrantOverride(
            code,
            name,
            0,
            "",
            new List<string>(),
            0,
            0,
            "",
            0,
            "",
            AdaptiveEquityGrantOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEquityGrantOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEquityGrantOverrideStatus next)
    {
        switch (Status)
        {
            case AdaptiveEquityGrantOverrideStatus.New:
                switch (next)
                {
                    case AdaptiveEquityGrantOverrideStatus.Done:
                        return true;
                }
                return false;
            case AdaptiveEquityGrantOverrideStatus.Queued:
                return next == AdaptiveEquityGrantOverrideStatus.Running || next == AdaptiveEquityGrantOverrideStatus.Done;
            case AdaptiveEquityGrantOverrideStatus.Running:
                switch (next)
                {
                    case AdaptiveEquityGrantOverrideStatus.Done:
                    case AdaptiveEquityGrantOverrideStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveEquityGrantOverrideStatus.Done:
                return next == AdaptiveEquityGrantOverrideStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveEquityGrantOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive equity grant override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
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

    public double NetTotal()
    {
        return Math.Round((double)GraceDays - (double)Sequence, 2);
    }

    public AdaptiveEquityGrantOverride Copy()
    {
        return new AdaptiveEquityGrantOverride(
            Code,
            Name,
            CreatedAt,
            ContactEmail,
            Tags,
            Attempts,
            Sequence,
            Segment,
            GraceDays,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
