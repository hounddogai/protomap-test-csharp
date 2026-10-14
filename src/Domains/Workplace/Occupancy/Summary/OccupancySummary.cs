namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy summary is in its life cycle.</summary>
public enum OccupancySummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One occupancy summary, with what the planners need to act on it.</summary>
public sealed class OccupancySummary(string code, string label, string name, bool pinned, int graceDays, int durationMinutes, string currency, Dictionary<string, string> attributes, OccupancySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int GraceDays { get; set; } = graceDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Currency { get; set; } = currency;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public OccupancySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy summary in status requested.</summary>
    public static OccupancySummary NewOccupancySummary(string code, string label)
    {
        return new OccupancySummary(
            code,
            label,
            "",
            false,
            0,
            0,
            "",
            new Dictionary<string, string>(),
            OccupancySummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancySummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancySummaryStatus next)
    {
        switch (Status)
        {
            case OccupancySummaryStatus.Requested:
                switch (next)
                {
                    case OccupancySummaryStatus.Revoked:
                    case OccupancySummaryStatus.Expired:
                        return true;
                }
                return false;
            case OccupancySummaryStatus.Granted:
                switch (next)
                {
                    case OccupancySummaryStatus.Expired:
                        return true;
                }
                return false;
            case OccupancySummaryStatus.Revoked:
                return next == OccupancySummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(OccupancySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays - (double)DurationMinutes, 2);
    }

    public OccupancySummary Copy()
    {
        return new OccupancySummary(
            Code,
            Label,
            Name,
            Pinned,
            GraceDays,
            DurationMinutes,
            Currency,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
