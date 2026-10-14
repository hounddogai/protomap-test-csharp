namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling merit cycle record is in its life cycle.</summary>
public enum RollingMeritCycleRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One rolling merit cycle record, with what the planners need to act on it.</summary>
public sealed class RollingMeritCycleRecord(string code, string title, double threshold, bool pinned, double hours, string contactEmail, string label, double budget, RollingMeritCycleRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public RollingMeritCycleRecordStatus Status { get; set; } = status;

    /// <summary>Starts a rolling merit cycle record in status created.</summary>
    public static RollingMeritCycleRecord NewRollingMeritCycleRecord(string code, string title)
    {
        return new RollingMeritCycleRecord(
            code,
            title,
            0.0,
            false,
            0.0,
            "",
            "",
            0.0,
            RollingMeritCycleRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMeritCycleRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMeritCycleRecordStatus next)
    {
        switch (Status)
        {
            case RollingMeritCycleRecordStatus.Created:
                switch (next)
                {
                    case RollingMeritCycleRecordStatus.Published:
                        return true;
                }
                return false;
            case RollingMeritCycleRecordStatus.Validated:
                switch (next)
                {
                    case RollingMeritCycleRecordStatus.Retired:
                        return true;
                }
                return false;
            case RollingMeritCycleRecordStatus.Published:
                return next == RollingMeritCycleRecordStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingMeritCycleRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling merit cycle record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold - Hours, 2);
    }

    public RollingMeritCycleRecord Copy()
    {
        return new RollingMeritCycleRecord(
            Code,
            Title,
            Threshold,
            Pinned,
            Hours,
            ContactEmail,
            Label,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
