namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span schedule is in its life cycle.</summary>
public enum SpanScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A span schedule as the span team keeps it.</summary>
public sealed class SpanSchedule(string code, string name, string department, string unit, string employeeId, int version, string reviewer, bool pinned, Dictionary<string, string> attributes, double discount, int sequence, SpanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public string EmployeeId { get; set; } = employeeId;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public int Sequence { get; set; } = sequence;
    public SpanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a span schedule in status created.</summary>
    public static SpanSchedule NewSpanSchedule(string code, string name)
    {
        return new SpanSchedule(
            code,
            name,
            "",
            "",
            "",
            0,
            "",
            false,
            new Dictionary<string, string>(),
            0.0,
            0,
            SpanScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanScheduleStatus next)
    {
        switch (Status)
        {
            case SpanScheduleStatus.Created:
                switch (next)
                {
                    case SpanScheduleStatus.Validated:
                    case SpanScheduleStatus.Published:
                        return true;
                }
                return false;
            case SpanScheduleStatus.Validated:
                return next == SpanScheduleStatus.Retired;
            case SpanScheduleStatus.Published:
                switch (next)
                {
                    case SpanScheduleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SpanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Version - (double)Sequence, 2);
    }

    public SpanSchedule Copy()
    {
        return new SpanSchedule(
            Code,
            Name,
            Department,
            Unit,
            EmployeeId,
            Version,
            Reviewer,
            Pinned,
            Attributes,
            Discount,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
