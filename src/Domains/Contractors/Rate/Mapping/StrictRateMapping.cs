namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict rate mapping is in its life cycle.</summary>
public enum StrictRateMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict rate mapping. Its code is unique within a tenant.</summary>
public sealed class StrictRateMapping(string code, string name, double threshold, double weight, double score, bool approved, int startsAt, string grade, string managerId, StrictRateMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public StrictRateMappingStatus Status { get; set; } = status;

    /// <summary>Starts a strict rate mapping in status requested.</summary>
    public static StrictRateMapping NewStrictRateMapping(string code, string name)
    {
        return new StrictRateMapping(code, name, 0.0, 0.0, 0.0, false, 0, "", "", StrictRateMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictRateMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictRateMappingStatus next)
    {
        switch (Status)
        {
            case StrictRateMappingStatus.Requested:
                switch (next)
                {
                    case StrictRateMappingStatus.Granted:
                        return true;
                }
                return false;
            case StrictRateMappingStatus.Granted:
                switch (next)
                {
                    case StrictRateMappingStatus.Revoked:
                    case StrictRateMappingStatus.Expired:
                        return true;
                }
                return false;
            case StrictRateMappingStatus.Revoked:
                return next == StrictRateMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictRateMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict rate mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Score - Weight, 2);
    }

    public StrictRateMapping Copy()
    {
        return new StrictRateMapping(
            Code,
            Name,
            Threshold,
            Weight,
            Score,
            Approved,
            StartsAt,
            Grade,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
