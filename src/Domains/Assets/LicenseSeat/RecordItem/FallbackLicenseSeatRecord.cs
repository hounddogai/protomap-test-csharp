namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat record is in its life cycle.</summary>
public enum FallbackLicenseSeatRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a fallback license seat record and its current status.</summary>
public sealed class FallbackLicenseSeatRecord(string code, string name, string timezone, int createdAt, int version, string source, FallbackLicenseSeatRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public FallbackLicenseSeatRecordStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat record in status planned.</summary>
    public static FallbackLicenseSeatRecord NewFallbackLicenseSeatRecord(string code, string name)
    {
        return new FallbackLicenseSeatRecord(code, name, "", 0, 0, "", FallbackLicenseSeatRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatRecordStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatRecordStatus.Planned:
                return next == FallbackLicenseSeatRecordStatus.Scheduled || next == FallbackLicenseSeatRecordStatus.InProgress;
            case FallbackLicenseSeatRecordStatus.Scheduled:
                return next == FallbackLicenseSeatRecordStatus.InProgress;
            case FallbackLicenseSeatRecordStatus.InProgress:
                return next == FallbackLicenseSeatRecordStatus.Cancelled;
            case FallbackLicenseSeatRecordStatus.Completed:
                switch (next)
                {
                    case FallbackLicenseSeatRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)CreatedAt + (double)Version, 2);
    }

    public FallbackLicenseSeatRecord Copy()
    {
        return new FallbackLicenseSeatRecord(Code, Name, Timezone, CreatedAt, Version, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
