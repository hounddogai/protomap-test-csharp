namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal occupancy preference is in its life cycle.</summary>
public enum SeasonalOccupancyPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal occupancy preference. Its code is unique within a tenant.</summary>
public sealed class SeasonalOccupancyPreference(string code, string label, double fte, Dictionary<string, string> attributes, string salaryBand, SeasonalOccupancyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string SalaryBand { get; set; } = salaryBand;
    public SeasonalOccupancyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal occupancy preference in status planned.</summary>
    public static SeasonalOccupancyPreference NewSeasonalOccupancyPreference(string code, string label)
    {
        return new SeasonalOccupancyPreference(
            code,
            label,
            0.0,
            new Dictionary<string, string>(),
            "",
            SeasonalOccupancyPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalOccupancyPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalOccupancyPreferenceStatus next)
    {
        switch (Status)
        {
            case SeasonalOccupancyPreferenceStatus.Planned:
                switch (next)
                {
                    case SeasonalOccupancyPreferenceStatus.Scheduled:
                    case SeasonalOccupancyPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalOccupancyPreferenceStatus.Scheduled:
                switch (next)
                {
                    case SeasonalOccupancyPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalOccupancyPreferenceStatus.InProgress:
                return next == SeasonalOccupancyPreferenceStatus.Completed;
            case SeasonalOccupancyPreferenceStatus.Completed:
                switch (next)
                {
                    case SeasonalOccupancyPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalOccupancyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal occupancy preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double MeasuredValue()
    {
        return Fte;
    }

    public SeasonalOccupancyPreference Copy()
    {
        return new SeasonalOccupancyPreference(Code, Label, Fte, Attributes, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
