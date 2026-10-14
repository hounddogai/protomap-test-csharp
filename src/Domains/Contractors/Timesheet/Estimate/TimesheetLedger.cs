namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet ledger is in its life cycle.</summary>
public enum TimesheetLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One timesheet ledger, with what the planners need to act on it.</summary>
public sealed class TimesheetLedger(string code, string label, string team, Dictionary<string, string> attributes, int version, string timezone, double rating, string employeeId, double threshold, List<string> tags, TimesheetLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public double Rating { get; set; } = rating;
    public string EmployeeId { get; set; } = employeeId;
    public double Threshold { get; set; } = threshold;
    public List<string> Tags { get; set; } = tags;
    public TimesheetLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet ledger in status created.</summary>
    public static TimesheetLedger NewTimesheetLedger(string code, string label)
    {
        return new TimesheetLedger(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            "",
            0.0,
            new List<string>(),
            TimesheetLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetLedgerStatus next)
    {
        switch (Status)
        {
            case TimesheetLedgerStatus.Created:
                return next == TimesheetLedgerStatus.Retired;
            case TimesheetLedgerStatus.Validated:
                switch (next)
                {
                    case TimesheetLedgerStatus.Published:
                    case TimesheetLedgerStatus.Retired:
                        return true;
                }
                return false;
            case TimesheetLedgerStatus.Published:
                switch (next)
                {
                    case TimesheetLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"timesheet ledger"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double EffectiveValue()
    {
        return Math.Round(Rating + (double)Version, 2);
    }

    public TimesheetLedger Copy()
    {
        return new TimesheetLedger(
            Code,
            Label,
            Team,
            Attributes,
            Version,
            Timezone,
            Rating,
            EmployeeId,
            Threshold,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
