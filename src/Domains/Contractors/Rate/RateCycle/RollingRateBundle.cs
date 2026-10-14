namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling rate bundle is in its life cycle.</summary>
public enum RollingRateBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling rate bundle as the rate team keeps it.</summary>
public sealed class RollingRateBundle(string code, string name, string channel, bool archived, string salaryBand, double ceiling, double discount, string department, int leaveDays, RollingRateBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public int LeaveDays { get; set; } = leaveDays;
    public RollingRateBundleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling rate bundle in status draft.</summary>
    public static RollingRateBundle NewRollingRateBundle(string code, string name)
    {
        return new RollingRateBundle(code, name, "", false, "", 0.0, 0.0, "", 0, RollingRateBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRateBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRateBundleStatus next)
    {
        switch (Status)
        {
            case RollingRateBundleStatus.Draft:
                switch (next)
                {
                    case RollingRateBundleStatus.Pending:
                    case RollingRateBundleStatus.Active:
                        return true;
                }
                return false;
            case RollingRateBundleStatus.Pending:
                return next == RollingRateBundleStatus.Suspended || next == RollingRateBundleStatus.Closed;
            case RollingRateBundleStatus.Active:
                return next == RollingRateBundleStatus.Suspended;
            case RollingRateBundleStatus.Suspended:
                switch (next)
                {
                    case RollingRateBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRateBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling rate bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling rate bundle"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Discount - Ceiling, 2);
    }

    public RollingRateBundle Copy()
    {
        return new RollingRateBundle(
            Code,
            Name,
            Channel,
            Archived,
            SalaryBand,
            Ceiling,
            Discount,
            Department,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
