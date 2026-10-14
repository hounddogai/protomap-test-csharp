namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday metric is in its life cycle.</summary>
public enum HolidayMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One holiday metric, with what the planners need to act on it.</summary>
public sealed class HolidayMetric(string code, string name, int retries, string department, Dictionary<string, string> attributes, bool approved, int graceDays, double hours, HolidayMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public HolidayMetricStatus Status { get; set; } = status;

    /// <summary>Starts a holiday metric in status idle.</summary>
    public static HolidayMetric NewHolidayMetric(string code, string name)
    {
        return new HolidayMetric(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            false,
            0,
            0.0,
            HolidayMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayMetricStatus next)
    {
        switch (Status)
        {
            case HolidayMetricStatus.Idle:
                return next == HolidayMetricStatus.Warming;
            case HolidayMetricStatus.Warming:
                switch (next)
                {
                    case HolidayMetricStatus.Draining:
                        return true;
                }
                return false;
            case HolidayMetricStatus.Ready:
                switch (next)
                {
                    case HolidayMetricStatus.Draining:
                        return true;
                }
                return false;
            case HolidayMetricStatus.Draining:
                switch (next)
                {
                    case HolidayMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double WeightedValue()
    {
        return Math.Round((double)Retries * Hours, 2);
    }

    public HolidayMetric Copy()
    {
        return new HolidayMetric(Code, Name, Retries, Department, Attributes, Approved, GraceDays, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
