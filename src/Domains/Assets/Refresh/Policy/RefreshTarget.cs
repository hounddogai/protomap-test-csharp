namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh target is in its life cycle.</summary>
public enum RefreshTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A refresh target as the refresh team keeps it.</summary>
public sealed class RefreshTarget(string code, string title, int version, bool pinned, double maximum, string jobCode, string grade, string currency, string unit, RefreshTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public RefreshTargetStatus Status { get; set; } = status;

    /// <summary>Starts a refresh target in status created.</summary>
    public static RefreshTarget NewRefreshTarget(string code, string title)
    {
        return new RefreshTarget(code, title, 0, false, 0.0, "", "", "", "", RefreshTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshTargetStatus next)
    {
        switch (Status)
        {
            case RefreshTargetStatus.Created:
                return next == RefreshTargetStatus.Published || next == RefreshTargetStatus.Retired;
            case RefreshTargetStatus.Validated:
                return next == RefreshTargetStatus.Published || next == RefreshTargetStatus.Retired;
            case RefreshTargetStatus.Published:
                switch (next)
                {
                    case RefreshTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Maximum + (double)Version, 2);
    }

    public RefreshTarget Copy()
    {
        return new RefreshTarget(Code, Title, Version, Pinned, Maximum, JobCode, Grade, Currency, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
