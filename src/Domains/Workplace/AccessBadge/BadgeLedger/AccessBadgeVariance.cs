namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge variance is in its life cycle.</summary>
public enum AccessBadgeVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A access badge variance as the access badge team keeps it.</summary>
public sealed class AccessBadgeVariance(string code, string name, double weight, string badge, double budget, double maximum, int windowDays, string location, string unit, AccessBadgeVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public AccessBadgeVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a access badge variance in status draft.</summary>
    public static AccessBadgeVariance NewAccessBadgeVariance(string code, string name)
    {
        return new AccessBadgeVariance(code, name, 0.0, "", 0.0, 0.0, 0, "", "", AccessBadgeVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeVarianceStatus next)
    {
        switch (Status)
        {
            case AccessBadgeVarianceStatus.Draft:
                switch (next)
                {
                    case AccessBadgeVarianceStatus.Active:
                        return true;
                }
                return false;
            case AccessBadgeVarianceStatus.Pending:
                return next == AccessBadgeVarianceStatus.Suspended || next == AccessBadgeVarianceStatus.Closed;
            case AccessBadgeVarianceStatus.Active:
                switch (next)
                {
                    case AccessBadgeVarianceStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgeVarianceStatus.Suspended:
                switch (next)
                {
                    case AccessBadgeVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"access badge variance"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Budget + Weight, 2);
    }

    public AccessBadgeVariance Copy()
    {
        return new AccessBadgeVariance(
            Code,
            Name,
            Weight,
            Badge,
            Budget,
            Maximum,
            WindowDays,
            Location,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
