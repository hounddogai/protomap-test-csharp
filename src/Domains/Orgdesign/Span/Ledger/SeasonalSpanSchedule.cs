namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal span schedule is in its life cycle.</summary>
public enum SeasonalSpanScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal span schedule as the span team keeps it.</summary>
public sealed class SeasonalSpanSchedule(string code, string name, double margin, bool approved, string timezone, string region, string source, bool archived, SeasonalSpanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public bool Approved { get; set; } = approved;
    public string Timezone { get; set; } = timezone;
    public string Region { get; set; } = region;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public SeasonalSpanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal span schedule in status requested.</summary>
    public static SeasonalSpanSchedule NewSeasonalSpanSchedule(string code, string name)
    {
        return new SeasonalSpanSchedule(
            code,
            name,
            0.0,
            false,
            "",
            "",
            "",
            false,
            SeasonalSpanScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSpanScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSpanScheduleStatus next)
    {
        switch (Status)
        {
            case SeasonalSpanScheduleStatus.Requested:
                return next == SeasonalSpanScheduleStatus.Revoked;
            case SeasonalSpanScheduleStatus.Granted:
                return next == SeasonalSpanScheduleStatus.Revoked || next == SeasonalSpanScheduleStatus.Expired;
            case SeasonalSpanScheduleStatus.Revoked:
                switch (next)
                {
                    case SeasonalSpanScheduleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalSpanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal span schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal span schedule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Margin;
    }

    public SeasonalSpanSchedule Copy()
    {
        return new SeasonalSpanSchedule(Code, Name, Margin, Approved, Timezone, Region, Source, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
