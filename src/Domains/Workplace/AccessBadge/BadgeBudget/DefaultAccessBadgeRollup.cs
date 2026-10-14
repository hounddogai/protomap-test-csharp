namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default access badge rollup is in its life cycle.</summary>
public enum DefaultAccessBadgeRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default access badge rollup, with what the planners need to act on it.</summary>
public sealed class DefaultAccessBadgeRollup(string code, string label, int quantity, int windowDays, bool pinned, DefaultAccessBadgeRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int WindowDays { get; set; } = windowDays;
    public bool Pinned { get; set; } = pinned;
    public DefaultAccessBadgeRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default access badge rollup in status draft.</summary>
    public static DefaultAccessBadgeRollup NewDefaultAccessBadgeRollup(string code, string label)
    {
        return new DefaultAccessBadgeRollup(code, label, 0, 0, false, DefaultAccessBadgeRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAccessBadgeRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAccessBadgeRollupStatus next)
    {
        switch (Status)
        {
            case DefaultAccessBadgeRollupStatus.Draft:
                return next == DefaultAccessBadgeRollupStatus.Closed;
            case DefaultAccessBadgeRollupStatus.Pending:
                return next == DefaultAccessBadgeRollupStatus.Closed;
            case DefaultAccessBadgeRollupStatus.Active:
                return next == DefaultAccessBadgeRollupStatus.Suspended || next == DefaultAccessBadgeRollupStatus.Closed;
            case DefaultAccessBadgeRollupStatus.Suspended:
                return next == DefaultAccessBadgeRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultAccessBadgeRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default access badge rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default access badge rollup"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)WindowDays * (double)Quantity, 2);
    }

    public DefaultAccessBadgeRollup Copy()
    {
        return new DefaultAccessBadgeRollup(Code, Label, Quantity, WindowDays, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
