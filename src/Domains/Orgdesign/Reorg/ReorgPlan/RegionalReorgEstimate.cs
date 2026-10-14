namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional reorg estimate is in its life cycle.</summary>
public enum RegionalReorgEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional reorg estimate and its current status.</summary>
public sealed class RegionalReorgEstimate(string code, string label, string currency, string segment, string skill, int endsAt, string department, double ratio, int retries, string ownerId, int version, RegionalReorgEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string Segment { get; set; } = segment;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public double Ratio { get; set; } = ratio;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public RegionalReorgEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a regional reorg estimate in status requested.</summary>
    public static RegionalReorgEstimate NewRegionalReorgEstimate(string code, string label)
    {
        return new RegionalReorgEstimate(
            code,
            label,
            "",
            "",
            "",
            0,
            "",
            0.0,
            0,
            "",
            0,
            RegionalReorgEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalReorgEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalReorgEstimateStatus next)
    {
        switch (Status)
        {
            case RegionalReorgEstimateStatus.Requested:
                switch (next)
                {
                    case RegionalReorgEstimateStatus.Revoked:
                    case RegionalReorgEstimateStatus.Expired:
                        return true;
                }
                return false;
            case RegionalReorgEstimateStatus.Granted:
                return next == RegionalReorgEstimateStatus.Revoked || next == RegionalReorgEstimateStatus.Expired;
            case RegionalReorgEstimateStatus.Revoked:
                switch (next)
                {
                    case RegionalReorgEstimateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalReorgEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional reorg estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Retries + (double)EndsAt, 2);
    }

    public RegionalReorgEstimate Copy()
    {
        return new RegionalReorgEstimate(
            Code,
            Label,
            Currency,
            Segment,
            Skill,
            EndsAt,
            Department,
            Ratio,
            Retries,
            OwnerId,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
