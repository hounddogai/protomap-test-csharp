namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default access badge override is in its life cycle.</summary>
public enum DefaultAccessBadgeOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default access badge override as the access badge team keeps it.</summary>
public sealed class DefaultAccessBadgeOverride(string code, string title, string badge, string jobCode, double discount, double minimum, DefaultAccessBadgeOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public double Minimum { get; set; } = minimum;
    public DefaultAccessBadgeOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a default access badge override in status proposed.</summary>
    public static DefaultAccessBadgeOverride NewDefaultAccessBadgeOverride(string code, string title)
    {
        return new DefaultAccessBadgeOverride(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            DefaultAccessBadgeOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAccessBadgeOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAccessBadgeOverrideStatus next)
    {
        switch (Status)
        {
            case DefaultAccessBadgeOverrideStatus.Proposed:
                return next == DefaultAccessBadgeOverrideStatus.Approved || next == DefaultAccessBadgeOverrideStatus.Withdrawn;
            case DefaultAccessBadgeOverrideStatus.Approved:
                switch (next)
                {
                    case DefaultAccessBadgeOverrideStatus.Rejected:
                    case DefaultAccessBadgeOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultAccessBadgeOverrideStatus.Rejected:
                return next == DefaultAccessBadgeOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultAccessBadgeOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default access badge override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default access badge override"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Discount * Minimum, 2);
    }

    public DefaultAccessBadgeOverride Copy()
    {
        return new DefaultAccessBadgeOverride(Code, Title, Badge, JobCode, Discount, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
