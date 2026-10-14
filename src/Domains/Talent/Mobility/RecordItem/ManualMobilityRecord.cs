namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual mobility record is in its life cycle.</summary>
public enum ManualMobilityRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One manual mobility record, with what the planners need to act on it.</summary>
public sealed class ManualMobilityRecord(string code, string label, string title, string timezone, string locale, double floorValue, double rating, ManualMobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public double Rating { get; set; } = rating;
    public ManualMobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a manual mobility record in status draft.</summary>
    public static ManualMobilityRecord NewManualMobilityRecord(string code, string label)
    {
        return new ManualMobilityRecord(code, label, "", "", "", 0.0, 0.0, ManualMobilityRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualMobilityRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualMobilityRecordStatus next)
    {
        switch (Status)
        {
            case ManualMobilityRecordStatus.Draft:
                switch (next)
                {
                    case ManualMobilityRecordStatus.Closed:
                        return true;
                }
                return false;
            case ManualMobilityRecordStatus.Pending:
                switch (next)
                {
                    case ManualMobilityRecordStatus.Active:
                    case ManualMobilityRecordStatus.Closed:
                        return true;
                }
                return false;
            case ManualMobilityRecordStatus.Active:
                switch (next)
                {
                    case ManualMobilityRecordStatus.Suspended:
                        return true;
                }
                return false;
            case ManualMobilityRecordStatus.Suspended:
                switch (next)
                {
                    case ManualMobilityRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualMobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating + FloorValue, 2);
    }

    public ManualMobilityRecord Copy()
    {
        return new ManualMobilityRecord(Code, Label, Title, Timezone, Locale, FloorValue, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
