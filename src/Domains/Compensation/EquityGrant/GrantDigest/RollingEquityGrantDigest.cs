namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling equity grant digest is in its life cycle.</summary>
public enum RollingEquityGrantDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rolling equity grant digest as the equity grant team keeps it.</summary>
public sealed class RollingEquityGrantDigest(string code, string label, int limit, string summary, string currency, string category, int tenureMonths, string name, int leaveDays, RollingEquityGrantDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public RollingEquityGrantDigestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling equity grant digest in status proposed.</summary>
    public static RollingEquityGrantDigest NewRollingEquityGrantDigest(string code, string label)
    {
        return new RollingEquityGrantDigest(
            code,
            label,
            0,
            "",
            "",
            "",
            0,
            "",
            0,
            RollingEquityGrantDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingEquityGrantDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingEquityGrantDigestStatus next)
    {
        switch (Status)
        {
            case RollingEquityGrantDigestStatus.Proposed:
                return next == RollingEquityGrantDigestStatus.Approved;
            case RollingEquityGrantDigestStatus.Approved:
                return next == RollingEquityGrantDigestStatus.Rejected || next == RollingEquityGrantDigestStatus.Withdrawn;
            case RollingEquityGrantDigestStatus.Rejected:
                switch (next)
                {
                    case RollingEquityGrantDigestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingEquityGrantDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling equity grant digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths * (double)LeaveDays, 2);
    }

    public RollingEquityGrantDigest Copy()
    {
        return new RollingEquityGrantDigest(
            Code,
            Label,
            Limit,
            Summary,
            Currency,
            Category,
            TenureMonths,
            Name,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
