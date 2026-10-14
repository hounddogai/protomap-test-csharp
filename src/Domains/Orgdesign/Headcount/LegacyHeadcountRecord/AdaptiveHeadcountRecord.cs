namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive headcount record is in its life cycle.</summary>
public enum AdaptiveHeadcountRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive headcount record and its current status.</summary>
public sealed class AdaptiveHeadcountRecord(string code, string label, int batchSize, double floorValue, string jobCode, string region, int updatedAt, bool approved, int sequence, double margin, int attempts, AdaptiveHeadcountRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public double FloorValue { get; set; } = floorValue;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public int Sequence { get; set; } = sequence;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public AdaptiveHeadcountRecordStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive headcount record in status requested.</summary>
    public static AdaptiveHeadcountRecord NewAdaptiveHeadcountRecord(string code, string label)
    {
        return new AdaptiveHeadcountRecord(
            code,
            label,
            0,
            0.0,
            "",
            "",
            0,
            false,
            0,
            0.0,
            0,
            AdaptiveHeadcountRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHeadcountRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHeadcountRecordStatus next)
    {
        switch (Status)
        {
            case AdaptiveHeadcountRecordStatus.Requested:
                return next == AdaptiveHeadcountRecordStatus.Expired;
            case AdaptiveHeadcountRecordStatus.Granted:
                return next == AdaptiveHeadcountRecordStatus.Expired;
            case AdaptiveHeadcountRecordStatus.Revoked:
                return next == AdaptiveHeadcountRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveHeadcountRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive headcount record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive headcount record"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Sequence * (double)UpdatedAt, 2);
    }

    public AdaptiveHeadcountRecord Copy()
    {
        return new AdaptiveHeadcountRecord(
            Code,
            Label,
            BatchSize,
            FloorValue,
            JobCode,
            Region,
            UpdatedAt,
            Approved,
            Sequence,
            Margin,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
