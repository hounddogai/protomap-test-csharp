namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional deduction schedule is in its life cycle.</summary>
public enum RegionalDeductionScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional deduction schedule, with what the planners need to act on it.</summary>
public sealed class RegionalDeductionSchedule(string code, string label, double floorValue, string department, int updatedAt, string externalRef, double threshold, string managerId, string name, double cost, Dictionary<string, string> attributes, RegionalDeductionScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Department { get; set; } = department;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public string ManagerId { get; set; } = managerId;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RegionalDeductionScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a regional deduction schedule in status proposed.</summary>
    public static RegionalDeductionSchedule NewRegionalDeductionSchedule(string code, string label)
    {
        return new RegionalDeductionSchedule(
            code,
            label,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            RegionalDeductionScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeductionScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeductionScheduleStatus next)
    {
        switch (Status)
        {
            case RegionalDeductionScheduleStatus.Proposed:
                switch (next)
                {
                    case RegionalDeductionScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case RegionalDeductionScheduleStatus.Approved:
                switch (next)
                {
                    case RegionalDeductionScheduleStatus.Rejected:
                    case RegionalDeductionScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalDeductionScheduleStatus.Rejected:
                return next == RegionalDeductionScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalDeductionScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional deduction schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double NetTotal()
    {
        return Math.Round(Threshold * FloorValue, 2);
    }

    public RegionalDeductionSchedule Copy()
    {
        return new RegionalDeductionSchedule(
            Code,
            Label,
            FloorValue,
            Department,
            UpdatedAt,
            ExternalRef,
            Threshold,
            ManagerId,
            Name,
            Cost,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
