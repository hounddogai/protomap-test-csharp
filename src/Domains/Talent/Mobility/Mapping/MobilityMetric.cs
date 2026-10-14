namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility metric is in its life cycle.</summary>
public enum MobilityMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a mobility metric and its current status.</summary>
public sealed class MobilityMetric(string code, string name, Dictionary<string, string> attributes, string department, string grade, bool approved, int quantity, int capacity, double minimum, string unit, bool active, MobilityMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public string Grade { get; set; } = grade;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public double Minimum { get; set; } = minimum;
    public string Unit { get; set; } = unit;
    public bool Active { get; set; } = active;
    public MobilityMetricStatus Status { get; set; } = status;

    /// <summary>Starts a mobility metric in status idle.</summary>
    public static MobilityMetric NewMobilityMetric(string code, string name)
    {
        return new MobilityMetric(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            false,
            0,
            0,
            0.0,
            "",
            false,
            MobilityMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityMetricStatus next)
    {
        switch (Status)
        {
            case MobilityMetricStatus.Idle:
                return next == MobilityMetricStatus.Warming || next == MobilityMetricStatus.Draining;
            case MobilityMetricStatus.Warming:
                switch (next)
                {
                    case MobilityMetricStatus.Stopped:
                        return true;
                }
                return false;
            case MobilityMetricStatus.Ready:
                return next == MobilityMetricStatus.Draining || next == MobilityMetricStatus.Stopped;
            case MobilityMetricStatus.Draining:
                return next == MobilityMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(MobilityMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)Capacity - (double)Quantity, 2);
    }

    public MobilityMetric Copy()
    {
        return new MobilityMetric(
            Code,
            Name,
            Attributes,
            Department,
            Grade,
            Approved,
            Quantity,
            Capacity,
            Minimum,
            Unit,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
