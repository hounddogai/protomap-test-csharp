namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback rate bundle is in its life cycle.</summary>
public enum FallbackRateBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback rate bundle. Its code is unique within a tenant.</summary>
public sealed class FallbackRateBundle(string code, string title, int endsAt, string checksum, string managerId, string team, double weight, int attempts, double minimum, bool active, FallbackRateBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public string ManagerId { get; set; } = managerId;
    public string Team { get; set; } = team;
    public double Weight { get; set; } = weight;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public FallbackRateBundleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback rate bundle in status requested.</summary>
    public static FallbackRateBundle NewFallbackRateBundle(string code, string title)
    {
        return new FallbackRateBundle(
            code,
            title,
            0,
            "",
            "",
            "",
            0.0,
            0,
            0.0,
            false,
            FallbackRateBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRateBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRateBundleStatus next)
    {
        switch (Status)
        {
            case FallbackRateBundleStatus.Requested:
                return next == FallbackRateBundleStatus.Expired;
            case FallbackRateBundleStatus.Granted:
                return next == FallbackRateBundleStatus.Revoked || next == FallbackRateBundleStatus.Expired;
            case FallbackRateBundleStatus.Revoked:
                return next == FallbackRateBundleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackRateBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback rate bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Minimum * Weight, 2);
    }

    public FallbackRateBundle Copy()
    {
        return new FallbackRateBundle(
            Code,
            Title,
            EndsAt,
            Checksum,
            ManagerId,
            Team,
            Weight,
            Attempts,
            Minimum,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
