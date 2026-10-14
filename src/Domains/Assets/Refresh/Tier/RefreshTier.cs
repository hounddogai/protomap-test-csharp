namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh tier is in its life cycle.</summary>
public enum RefreshTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A refresh tier as the refresh team keeps it.</summary>
public sealed class RefreshTier(string code, string title, double hours, int limit, double minimum, RefreshTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public RefreshTierStatus Status { get; set; } = status;

    /// <summary>Starts a refresh tier in status created.</summary>
    public static RefreshTier NewRefreshTier(string code, string title)
    {
        return new RefreshTier(code, title, 0.0, 0, 0.0, RefreshTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshTierStatus next)
    {
        switch (Status)
        {
            case RefreshTierStatus.Created:
                return next == RefreshTierStatus.Published || next == RefreshTierStatus.Retired;
            case RefreshTierStatus.Validated:
                return next == RefreshTierStatus.Retired;
            case RefreshTierStatus.Published:
                return next == RefreshTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RefreshTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(Hours - Minimum, 2);
    }

    public RefreshTier Copy()
    {
        return new RefreshTier(Code, Title, Hours, Limit, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
