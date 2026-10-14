namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global asset return record is in its life cycle.</summary>
public enum GlobalAssetReturnRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One global asset return record, with what the planners need to act on it.</summary>
public sealed class GlobalAssetReturnRecord(string code, string name, int startsAt, bool archived, int expiresAt, string checksum, string segment, int capacity, bool pinned, string jobCode, GlobalAssetReturnRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public string Segment { get; set; } = segment;
    public int Capacity { get; set; } = capacity;
    public bool Pinned { get; set; } = pinned;
    public string JobCode { get; set; } = jobCode;
    public GlobalAssetReturnRecordStatus Status { get; set; } = status;

    /// <summary>Starts a global asset return record in status proposed.</summary>
    public static GlobalAssetReturnRecord NewGlobalAssetReturnRecord(string code, string name)
    {
        return new GlobalAssetReturnRecord(
            code,
            name,
            0,
            false,
            0,
            "",
            "",
            0,
            false,
            "",
            GlobalAssetReturnRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAssetReturnRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAssetReturnRecordStatus next)
    {
        switch (Status)
        {
            case GlobalAssetReturnRecordStatus.Proposed:
                return next == GlobalAssetReturnRecordStatus.Withdrawn;
            case GlobalAssetReturnRecordStatus.Approved:
                switch (next)
                {
                    case GlobalAssetReturnRecordStatus.Rejected:
                    case GlobalAssetReturnRecordStatus.Withdrawn:
                        return true;
                }
                return false;
            case GlobalAssetReturnRecordStatus.Rejected:
                switch (next)
                {
                    case GlobalAssetReturnRecordStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAssetReturnRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global asset return record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt + (double)Capacity, 2);
    }

    public GlobalAssetReturnRecord Copy()
    {
        return new GlobalAssetReturnRecord(
            Code,
            Name,
            StartsAt,
            Archived,
            ExpiresAt,
            Checksum,
            Segment,
            Capacity,
            Pinned,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
