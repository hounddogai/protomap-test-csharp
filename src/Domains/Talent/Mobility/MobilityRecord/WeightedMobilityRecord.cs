namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted mobility record is in its life cycle.</summary>
public enum WeightedMobilityRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted mobility record. Its code is unique within a tenant.</summary>
public sealed class WeightedMobilityRecord(string code, string name, int limit, double budget, double ceiling, string team, WeightedMobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public string Team { get; set; } = team;
    public WeightedMobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a weighted mobility record in status requested.</summary>
    public static WeightedMobilityRecord NewWeightedMobilityRecord(string code, string name)
    {
        return new WeightedMobilityRecord(code, name, 0, 0.0, 0.0, "", WeightedMobilityRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedMobilityRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedMobilityRecordStatus next)
    {
        switch (Status)
        {
            case WeightedMobilityRecordStatus.Requested:
                return next == WeightedMobilityRecordStatus.Granted;
            case WeightedMobilityRecordStatus.Granted:
                return next == WeightedMobilityRecordStatus.Revoked || next == WeightedMobilityRecordStatus.Expired;
            case WeightedMobilityRecordStatus.Revoked:
                return next == WeightedMobilityRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedMobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Budget * Ceiling, 2);
    }

    public WeightedMobilityRecord Copy()
    {
        return new WeightedMobilityRecord(Code, Name, Limit, Budget, Ceiling, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
