namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling laptop baseline is in its life cycle.</summary>
public enum RollingLaptopBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling laptop baseline. Its code is unique within a tenant.</summary>
public sealed class RollingLaptopBaseline(string code, string name, string timezone, bool pinned, int leaveDays, string region, double maximum, string notes, string unit, RollingLaptopBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public double Maximum { get; set; } = maximum;
    public string Notes { get; set; } = notes;
    public string Unit { get; set; } = unit;
    public RollingLaptopBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a rolling laptop baseline in status open.</summary>
    public static RollingLaptopBaseline NewRollingLaptopBaseline(string code, string name)
    {
        return new RollingLaptopBaseline(
            code,
            name,
            "",
            false,
            0,
            "",
            0.0,
            "",
            "",
            RollingLaptopBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLaptopBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLaptopBaselineStatus next)
    {
        switch (Status)
        {
            case RollingLaptopBaselineStatus.Open:
                return next == RollingLaptopBaselineStatus.Resolved;
            case RollingLaptopBaselineStatus.InReview:
                switch (next)
                {
                    case RollingLaptopBaselineStatus.Resolved:
                    case RollingLaptopBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case RollingLaptopBaselineStatus.Resolved:
                switch (next)
                {
                    case RollingLaptopBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case RollingLaptopBaselineStatus.Reopened:
                return next == RollingLaptopBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingLaptopBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling laptop baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling laptop baseline"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays + Maximum, 2);
    }

    public RollingLaptopBaseline Copy()
    {
        return new RollingLaptopBaseline(
            Code,
            Name,
            Timezone,
            Pinned,
            LeaveDays,
            Region,
            Maximum,
            Notes,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
