namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount metric is in its life cycle.</summary>
public enum HeadcountMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a headcount metric and its current status.</summary>
public sealed class HeadcountMetric(string code, string name, int attempts, string location, bool visible, string notes, bool active, int endsAt, string channel, int startsAt, HeadcountMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Location { get; set; } = location;
    public bool Visible { get; set; } = visible;
    public string Notes { get; set; } = notes;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public int StartsAt { get; set; } = startsAt;
    public HeadcountMetricStatus Status { get; set; } = status;

    /// <summary>Starts a headcount metric in status requested.</summary>
    public static HeadcountMetric NewHeadcountMetric(string code, string name)
    {
        return new HeadcountMetric(code, name, 0, "", false, "", false, 0, "", 0, HeadcountMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountMetricStatus next)
    {
        switch (Status)
        {
            case HeadcountMetricStatus.Requested:
                switch (next)
                {
                    case HeadcountMetricStatus.Granted:
                    case HeadcountMetricStatus.Revoked:
                        return true;
                }
                return false;
            case HeadcountMetricStatus.Granted:
                switch (next)
                {
                    case HeadcountMetricStatus.Revoked:
                    case HeadcountMetricStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountMetricStatus.Revoked:
                switch (next)
                {
                    case HeadcountMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)EndsAt * (double)StartsAt, 2);
    }

    public HeadcountMetric Copy()
    {
        return new HeadcountMetric(
            Code,
            Name,
            Attempts,
            Location,
            Visible,
            Notes,
            Active,
            EndsAt,
            Channel,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
