namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive headcount exception is in its life cycle.</summary>
public enum AdaptiveHeadcountExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive headcount exception. Its code is unique within a tenant.</summary>
public sealed class AdaptiveHeadcountException(string code, string name, string region, int leaveDays, string ownerId, double rating, double rate, AdaptiveHeadcountExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public AdaptiveHeadcountExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive headcount exception in status requested.</summary>
    public static AdaptiveHeadcountException NewAdaptiveHeadcountException(string code, string name)
    {
        return new AdaptiveHeadcountException(
            code,
            name,
            "",
            0,
            "",
            0.0,
            0.0,
            AdaptiveHeadcountExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHeadcountExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHeadcountExceptionStatus next)
    {
        switch (Status)
        {
            case AdaptiveHeadcountExceptionStatus.Requested:
                switch (next)
                {
                    case AdaptiveHeadcountExceptionStatus.Granted:
                    case AdaptiveHeadcountExceptionStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveHeadcountExceptionStatus.Granted:
                return next == AdaptiveHeadcountExceptionStatus.Revoked || next == AdaptiveHeadcountExceptionStatus.Expired;
            case AdaptiveHeadcountExceptionStatus.Revoked:
                return next == AdaptiveHeadcountExceptionStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveHeadcountExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive headcount exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Rating - (double)LeaveDays, 2);
    }

    public AdaptiveHeadcountException Copy()
    {
        return new AdaptiveHeadcountException(Code, Name, Region, LeaveDays, OwnerId, Rating, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
