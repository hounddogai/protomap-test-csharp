namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default access badge bundle is in its life cycle.</summary>
public enum DefaultAccessBadgeBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default access badge bundle as the access badge team keeps it.</summary>
public sealed class DefaultAccessBadgeBundle(string code, string name, int attempts, int capacity, double minimum, double weight, int priority, bool visible, string locale, double rate, DefaultAccessBadgeBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public double Weight { get; set; } = weight;
    public int Priority { get; set; } = priority;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public double Rate { get; set; } = rate;
    public DefaultAccessBadgeBundleStatus Status { get; set; } = status;

    /// <summary>Starts a default access badge bundle in status planned.</summary>
    public static DefaultAccessBadgeBundle NewDefaultAccessBadgeBundle(string code, string name)
    {
        return new DefaultAccessBadgeBundle(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            0,
            false,
            "",
            0.0,
            DefaultAccessBadgeBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAccessBadgeBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAccessBadgeBundleStatus next)
    {
        switch (Status)
        {
            case DefaultAccessBadgeBundleStatus.Planned:
                switch (next)
                {
                    case DefaultAccessBadgeBundleStatus.InProgress:
                        return true;
                }
                return false;
            case DefaultAccessBadgeBundleStatus.Scheduled:
                return next == DefaultAccessBadgeBundleStatus.InProgress || next == DefaultAccessBadgeBundleStatus.Completed;
            case DefaultAccessBadgeBundleStatus.InProgress:
                switch (next)
                {
                    case DefaultAccessBadgeBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultAccessBadgeBundleStatus.Completed:
                switch (next)
                {
                    case DefaultAccessBadgeBundleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAccessBadgeBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default access badge bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rate * (double)Attempts, 2);
    }

    public DefaultAccessBadgeBundle Copy()
    {
        return new DefaultAccessBadgeBundle(
            Code,
            Name,
            Attempts,
            Capacity,
            Minimum,
            Weight,
            Priority,
            Visible,
            Locale,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
