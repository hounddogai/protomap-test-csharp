namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global occupancy record is in its life cycle.</summary>
public enum GlobalOccupancyRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One global occupancy record, with what the planners need to act on it.</summary>
public sealed class GlobalOccupancyRecord(string code, string name, string notes, string externalRef, int tenureMonths, double score, GlobalOccupancyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string ExternalRef { get; set; } = externalRef;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Score { get; set; } = score;
    public GlobalOccupancyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a global occupancy record in status created.</summary>
    public static GlobalOccupancyRecord NewGlobalOccupancyRecord(string code, string name)
    {
        return new GlobalOccupancyRecord(code, name, "", "", 0, 0.0, GlobalOccupancyRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalOccupancyRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalOccupancyRecordStatus next)
    {
        switch (Status)
        {
            case GlobalOccupancyRecordStatus.Created:
                switch (next)
                {
                    case GlobalOccupancyRecordStatus.Validated:
                    case GlobalOccupancyRecordStatus.Retired:
                        return true;
                }
                return false;
            case GlobalOccupancyRecordStatus.Validated:
                return next == GlobalOccupancyRecordStatus.Published || next == GlobalOccupancyRecordStatus.Retired;
            case GlobalOccupancyRecordStatus.Published:
                switch (next)
                {
                    case GlobalOccupancyRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalOccupancyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global occupancy record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Score - (double)TenureMonths, 2);
    }

    public GlobalOccupancyRecord Copy()
    {
        return new GlobalOccupancyRecord(Code, Name, Notes, ExternalRef, TenureMonths, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
