namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy holiday group is in its life cycle.</summary>
public enum LegacyHolidayGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy holiday group. Its code is unique within a tenant.</summary>
public sealed class LegacyHolidayGroup(string code, string label, string location, List<string> tags, int limit, int endsAt, string channel, string country, int batchSize, double score, LegacyHolidayGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public int BatchSize { get; set; } = batchSize;
    public double Score { get; set; } = score;
    public LegacyHolidayGroupStatus Status { get; set; } = status;

    /// <summary>Starts a legacy holiday group in status requested.</summary>
    public static LegacyHolidayGroup NewLegacyHolidayGroup(string code, string label)
    {
        return new LegacyHolidayGroup(
            code,
            label,
            "",
            new List<string>(),
            0,
            0,
            "",
            "",
            0,
            0.0,
            LegacyHolidayGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHolidayGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHolidayGroupStatus next)
    {
        switch (Status)
        {
            case LegacyHolidayGroupStatus.Requested:
                return next == LegacyHolidayGroupStatus.Granted || next == LegacyHolidayGroupStatus.Expired;
            case LegacyHolidayGroupStatus.Granted:
                return next == LegacyHolidayGroupStatus.Revoked || next == LegacyHolidayGroupStatus.Expired;
            case LegacyHolidayGroupStatus.Revoked:
                return next == LegacyHolidayGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LegacyHolidayGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy holiday group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedVolume()
    {
        return Math.Round((double)BatchSize - (double)Limit, 2);
    }

    public LegacyHolidayGroup Copy()
    {
        return new LegacyHolidayGroup(
            Code,
            Label,
            Location,
            Tags,
            Limit,
            EndsAt,
            Channel,
            Country,
            BatchSize,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
