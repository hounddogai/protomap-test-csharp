namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive equity grant target is in its life cycle.</summary>
public enum AdaptiveEquityGrantTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive equity grant target, with what the planners need to act on it.</summary>
public sealed class AdaptiveEquityGrantTarget(string code, string name, double minimum, int priority, string country, string checksum, string title, int version, string contactEmail, bool visible, string locale, AdaptiveEquityGrantTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int Priority { get; set; } = priority;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public AdaptiveEquityGrantTargetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive equity grant target in status idle.</summary>
    public static AdaptiveEquityGrantTarget NewAdaptiveEquityGrantTarget(string code, string name)
    {
        return new AdaptiveEquityGrantTarget(
            code,
            name,
            0.0,
            0,
            "",
            "",
            "",
            0,
            "",
            false,
            "",
            AdaptiveEquityGrantTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEquityGrantTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEquityGrantTargetStatus next)
    {
        switch (Status)
        {
            case AdaptiveEquityGrantTargetStatus.Idle:
                return next == AdaptiveEquityGrantTargetStatus.Stopped;
            case AdaptiveEquityGrantTargetStatus.Warming:
                switch (next)
                {
                    case AdaptiveEquityGrantTargetStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveEquityGrantTargetStatus.Ready:
                return next == AdaptiveEquityGrantTargetStatus.Stopped;
            case AdaptiveEquityGrantTargetStatus.Draining:
                return next == AdaptiveEquityGrantTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveEquityGrantTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive equity grant target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Priority * (double)Version, 2);
    }

    public AdaptiveEquityGrantTarget Copy()
    {
        return new AdaptiveEquityGrantTarget(
            Code,
            Name,
            Minimum,
            Priority,
            Country,
            Checksum,
            Title,
            Version,
            ContactEmail,
            Visible,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
