namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility manifest is in its life cycle.</summary>
public enum ScheduledMobilityManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled mobility manifest as the mobility team keeps it.</summary>
public sealed class ScheduledMobilityManifest(string code, string label, double budget, string description, string grade, string locale, string department, double minimum, int capacity, string reviewer, ScheduledMobilityManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Description { get; set; } = description;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public string Department { get; set; } = department;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public ScheduledMobilityManifestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility manifest in status requested.</summary>
    public static ScheduledMobilityManifest NewScheduledMobilityManifest(string code, string label)
    {
        return new ScheduledMobilityManifest(
            code,
            label,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            0,
            "",
            ScheduledMobilityManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityManifestStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityManifestStatus.Requested:
                switch (next)
                {
                    case ScheduledMobilityManifestStatus.Granted:
                    case ScheduledMobilityManifestStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledMobilityManifestStatus.Granted:
                return next == ScheduledMobilityManifestStatus.Revoked || next == ScheduledMobilityManifestStatus.Expired;
            case ScheduledMobilityManifestStatus.Revoked:
                switch (next)
                {
                    case ScheduledMobilityManifestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Budget - Minimum, 2);
    }

    public ScheduledMobilityManifest Copy()
    {
        return new ScheduledMobilityManifest(
            Code,
            Label,
            Budget,
            Description,
            Grade,
            Locale,
            Department,
            Minimum,
            Capacity,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
