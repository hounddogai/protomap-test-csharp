namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default mobility record is in its life cycle.</summary>
public enum DefaultMobilityRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default mobility record as the mobility team keeps it.</summary>
public sealed class DefaultMobilityRecord(string code, string label, int windowDays, bool archived, double margin, string segment, double ratio, string notes, int startsAt, string currency, string salaryBand, DefaultMobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public bool Archived { get; set; } = archived;
    public double Margin { get; set; } = margin;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public string Notes { get; set; } = notes;
    public int StartsAt { get; set; } = startsAt;
    public string Currency { get; set; } = currency;
    public string SalaryBand { get; set; } = salaryBand;
    public DefaultMobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a default mobility record in status proposed.</summary>
    public static DefaultMobilityRecord NewDefaultMobilityRecord(string code, string label)
    {
        return new DefaultMobilityRecord(
            code,
            label,
            0,
            false,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            "",
            DefaultMobilityRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultMobilityRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultMobilityRecordStatus next)
    {
        switch (Status)
        {
            case DefaultMobilityRecordStatus.Proposed:
                return next == DefaultMobilityRecordStatus.Approved || next == DefaultMobilityRecordStatus.Withdrawn;
            case DefaultMobilityRecordStatus.Approved:
                switch (next)
                {
                    case DefaultMobilityRecordStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultMobilityRecordStatus.Rejected:
                return next == DefaultMobilityRecordStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultMobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays * Ratio, 2);
    }

    public DefaultMobilityRecord Copy()
    {
        return new DefaultMobilityRecord(
            Code,
            Label,
            WindowDays,
            Archived,
            Margin,
            Segment,
            Ratio,
            Notes,
            StartsAt,
            Currency,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
