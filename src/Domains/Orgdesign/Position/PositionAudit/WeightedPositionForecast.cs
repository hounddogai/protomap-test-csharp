namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted position forecast is in its life cycle.</summary>
public enum WeightedPositionForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted position forecast. Its code is unique within a tenant.</summary>
public sealed class WeightedPositionForecast(string code, string label, int version, string timezone, string location, double ratio, string checksum, bool approved, string source, string badge, WeightedPositionForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public string Location { get; set; } = location;
    public double Ratio { get; set; } = ratio;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public string Badge { get; set; } = badge;
    public WeightedPositionForecastStatus Status { get; set; } = status;

    /// <summary>Starts a weighted position forecast in status proposed.</summary>
    public static WeightedPositionForecast NewWeightedPositionForecast(string code, string label)
    {
        return new WeightedPositionForecast(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            false,
            "",
            "",
            WeightedPositionForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPositionForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPositionForecastStatus next)
    {
        switch (Status)
        {
            case WeightedPositionForecastStatus.Proposed:
                return next == WeightedPositionForecastStatus.Rejected || next == WeightedPositionForecastStatus.Withdrawn;
            case WeightedPositionForecastStatus.Approved:
                switch (next)
                {
                    case WeightedPositionForecastStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedPositionForecastStatus.Rejected:
                return next == WeightedPositionForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedPositionForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted position forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version * Ratio, 2);
    }

    public WeightedPositionForecast Copy()
    {
        return new WeightedPositionForecast(
            Code,
            Label,
            Version,
            Timezone,
            Location,
            Ratio,
            Checksum,
            Approved,
            Source,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
