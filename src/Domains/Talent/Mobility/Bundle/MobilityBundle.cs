namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility bundle is in its life cycle.</summary>
public enum MobilityBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a mobility bundle and its current status.</summary>
public sealed class MobilityBundle(string code, string title, double floorValue, int retries, double budget, int endsAt, double fte, int priority, string grade, int tenureMonths, MobilityBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public double Fte { get; set; } = fte;
    public int Priority { get; set; } = priority;
    public string Grade { get; set; } = grade;
    public int TenureMonths { get; set; } = tenureMonths;
    public MobilityBundleStatus Status { get; set; } = status;

    /// <summary>Starts a mobility bundle in status requested.</summary>
    public static MobilityBundle NewMobilityBundle(string code, string title)
    {
        return new MobilityBundle(code, title, 0.0, 0, 0.0, 0, 0.0, 0, "", 0, MobilityBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityBundleStatus next)
    {
        switch (Status)
        {
            case MobilityBundleStatus.Requested:
                switch (next)
                {
                    case MobilityBundleStatus.Revoked:
                        return true;
                }
                return false;
            case MobilityBundleStatus.Granted:
                return next == MobilityBundleStatus.Revoked || next == MobilityBundleStatus.Expired;
            case MobilityBundleStatus.Revoked:
                return next == MobilityBundleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(MobilityBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Budget * (double)Priority, 2);
    }

    public MobilityBundle Copy()
    {
        return new MobilityBundle(
            Code,
            Title,
            FloorValue,
            Retries,
            Budget,
            EndsAt,
            Fte,
            Priority,
            Grade,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
