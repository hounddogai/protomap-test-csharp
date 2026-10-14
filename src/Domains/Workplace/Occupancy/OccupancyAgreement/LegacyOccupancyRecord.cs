namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy occupancy record is in its life cycle.</summary>
public enum LegacyOccupancyRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a legacy occupancy record and its current status.</summary>
public sealed class LegacyOccupancyRecord(string code, string name, string category, double weight, double hours, string summary, int expiresAt, double fte, LegacyOccupancyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public string Summary { get; set; } = summary;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Fte { get; set; } = fte;
    public LegacyOccupancyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy occupancy record in status created.</summary>
    public static LegacyOccupancyRecord NewLegacyOccupancyRecord(string code, string name)
    {
        return new LegacyOccupancyRecord(code, name, "", 0.0, 0.0, "", 0, 0.0, LegacyOccupancyRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOccupancyRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOccupancyRecordStatus next)
    {
        switch (Status)
        {
            case LegacyOccupancyRecordStatus.Created:
                switch (next)
                {
                    case LegacyOccupancyRecordStatus.Retired:
                        return true;
                }
                return false;
            case LegacyOccupancyRecordStatus.Validated:
                return next == LegacyOccupancyRecordStatus.Published;
            case LegacyOccupancyRecordStatus.Published:
                switch (next)
                {
                    case LegacyOccupancyRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOccupancyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy occupancy record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte - Hours, 2);
    }

    public LegacyOccupancyRecord Copy()
    {
        return new LegacyOccupancyRecord(Code, Name, Category, Weight, Hours, Summary, ExpiresAt, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
