namespace Acme.Workforce.Talent.Nomination.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive nomination target is in its life cycle.</summary>
public enum AdaptiveNominationTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive nomination target, with what the planners need to act on it.</summary>
public sealed class AdaptiveNominationTarget(string code, string name, double minimum, string source, int graceDays, string locale, double cost, double discount, string segment, string description, AdaptiveNominationTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public string Locale { get; set; } = locale;
    public double Cost { get; set; } = cost;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public AdaptiveNominationTargetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive nomination target in status planned.</summary>
    public static AdaptiveNominationTarget NewAdaptiveNominationTarget(string code, string name)
    {
        return new AdaptiveNominationTarget(
            code,
            name,
            0.0,
            "",
            0,
            "",
            0.0,
            0.0,
            "",
            "",
            AdaptiveNominationTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveNominationTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveNominationTargetStatus next)
    {
        switch (Status)
        {
            case AdaptiveNominationTargetStatus.Planned:
                switch (next)
                {
                    case AdaptiveNominationTargetStatus.Scheduled:
                    case AdaptiveNominationTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveNominationTargetStatus.Scheduled:
                return next == AdaptiveNominationTargetStatus.InProgress;
            case AdaptiveNominationTargetStatus.InProgress:
                switch (next)
                {
                    case AdaptiveNominationTargetStatus.Completed:
                    case AdaptiveNominationTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveNominationTargetStatus.Completed:
                return next == AdaptiveNominationTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveNominationTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive nomination target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Cost - (double)GraceDays, 2);
    }

    public AdaptiveNominationTarget Copy()
    {
        return new AdaptiveNominationTarget(
            Code,
            Name,
            Minimum,
            Source,
            GraceDays,
            Locale,
            Cost,
            Discount,
            Segment,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
