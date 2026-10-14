namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped desk record is in its life cycle.</summary>
public enum CappedDeskRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped desk record, with what the planners need to act on it.</summary>
public sealed class CappedDeskRecord(string code, string name, string segment, string location, int createdAt, int expiresAt, bool approved, double threshold, string externalRef, CappedDeskRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public CappedDeskRecordStatus Status { get; set; } = status;

    /// <summary>Starts a capped desk record in status created.</summary>
    public static CappedDeskRecord NewCappedDeskRecord(string code, string name)
    {
        return new CappedDeskRecord(code, name, "", "", 0, 0, false, 0.0, "", CappedDeskRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeskRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeskRecordStatus next)
    {
        switch (Status)
        {
            case CappedDeskRecordStatus.Created:
                switch (next)
                {
                    case CappedDeskRecordStatus.Validated:
                    case CappedDeskRecordStatus.Retired:
                        return true;
                }
                return false;
            case CappedDeskRecordStatus.Validated:
                return next == CappedDeskRecordStatus.Published;
            case CappedDeskRecordStatus.Published:
                switch (next)
                {
                    case CappedDeskRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedDeskRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped desk record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)ExpiresAt + Threshold, 2);
    }

    public CappedDeskRecord Copy()
    {
        return new CappedDeskRecord(
            Code,
            Name,
            Segment,
            Location,
            CreatedAt,
            ExpiresAt,
            Approved,
            Threshold,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
