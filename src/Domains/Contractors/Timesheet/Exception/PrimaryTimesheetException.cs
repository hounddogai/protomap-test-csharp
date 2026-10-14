namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary timesheet exception is in its life cycle.</summary>
public enum PrimaryTimesheetExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary timesheet exception. Its code is unique within a tenant.</summary>
public sealed class PrimaryTimesheetException(string code, string name, int version, string source, List<string> tags, double rating, Dictionary<string, string> attributes, double ceiling, PrimaryTimesheetExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ceiling { get; set; } = ceiling;
    public PrimaryTimesheetExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a primary timesheet exception in status idle.</summary>
    public static PrimaryTimesheetException NewPrimaryTimesheetException(string code, string name)
    {
        return new PrimaryTimesheetException(
            code,
            name,
            0,
            "",
            new List<string>(),
            0.0,
            new Dictionary<string, string>(),
            0.0,
            PrimaryTimesheetExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTimesheetExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTimesheetExceptionStatus next)
    {
        switch (Status)
        {
            case PrimaryTimesheetExceptionStatus.Idle:
                return next == PrimaryTimesheetExceptionStatus.Draining;
            case PrimaryTimesheetExceptionStatus.Warming:
                switch (next)
                {
                    case PrimaryTimesheetExceptionStatus.Ready:
                    case PrimaryTimesheetExceptionStatus.Draining:
                        return true;
                }
                return false;
            case PrimaryTimesheetExceptionStatus.Ready:
                switch (next)
                {
                    case PrimaryTimesheetExceptionStatus.Draining:
                    case PrimaryTimesheetExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryTimesheetExceptionStatus.Draining:
                return next == PrimaryTimesheetExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryTimesheetExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary timesheet exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double NetTotal()
    {
        return Math.Round(Rating - Ceiling, 2);
    }

    public PrimaryTimesheetException Copy()
    {
        return new PrimaryTimesheetException(Code, Name, Version, Source, Tags, Rating, Attributes, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
