namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle threshold is in its life cycle.</summary>
public enum MeritCycleThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle threshold as the merit cycle team keeps it.</summary>
public sealed class MeritCycleThreshold(string code, string title, int version, Dictionary<string, string> attributes, string locale, string name, double cost, string grade, double weight, int capacity, MeritCycleThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Locale { get; set; } = locale;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public MeritCycleThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle threshold in status proposed.</summary>
    public static MeritCycleThreshold NewMeritCycleThreshold(string code, string title)
    {
        return new MeritCycleThreshold(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            MeritCycleThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleThresholdStatus next)
    {
        switch (Status)
        {
            case MeritCycleThresholdStatus.Proposed:
                return next == MeritCycleThresholdStatus.Withdrawn;
            case MeritCycleThresholdStatus.Approved:
                return next == MeritCycleThresholdStatus.Rejected || next == MeritCycleThresholdStatus.Withdrawn;
            case MeritCycleThresholdStatus.Rejected:
                switch (next)
                {
                    case MeritCycleThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"merit cycle threshold"} {Code}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Cost * Weight, 2);
    }

    public MeritCycleThreshold Copy()
    {
        return new MeritCycleThreshold(
            Code,
            Title,
            Version,
            Attributes,
            Locale,
            Name,
            Cost,
            Grade,
            Weight,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
