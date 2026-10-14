namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy estimate is in its life cycle.</summary>
public enum OccupancyEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A occupancy estimate. Its code is unique within a tenant.</summary>
public sealed class OccupancyEstimate(string code, string title, string location, string locale, int batchSize, int leaveDays, string source, int version, double weight, string label, double fte, OccupancyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Locale { get; set; } = locale;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public double Weight { get; set; } = weight;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public OccupancyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy estimate in status new.</summary>
    public static OccupancyEstimate NewOccupancyEstimate(string code, string title)
    {
        return new OccupancyEstimate(code, title, "", "", 0, 0, "", 0, 0.0, "", 0.0, OccupancyEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyEstimateStatus next)
    {
        switch (Status)
        {
            case OccupancyEstimateStatus.New:
                switch (next)
                {
                    case OccupancyEstimateStatus.Running:
                        return true;
                }
                return false;
            case OccupancyEstimateStatus.Queued:
                switch (next)
                {
                    case OccupancyEstimateStatus.Running:
                        return true;
                }
                return false;
            case OccupancyEstimateStatus.Running:
                return next == OccupancyEstimateStatus.Done;
            case OccupancyEstimateStatus.Done:
                return next == OccupancyEstimateStatus.Failed;
        }
        return false;
    }

    public void MoveTo(OccupancyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Version + Weight, 2);
    }

    public OccupancyEstimate Copy()
    {
        return new OccupancyEstimate(
            Code,
            Title,
            Location,
            Locale,
            BatchSize,
            LeaveDays,
            Source,
            Version,
            Weight,
            Label,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
