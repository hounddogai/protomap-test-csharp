namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional career path rollup is in its life cycle.</summary>
public enum RegionalCareerPathRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a regional career path rollup and its current status.</summary>
public sealed class RegionalCareerPathRollup(string code, string title, string notes, int version, string badge, string country, string department, int tenureMonths, Dictionary<string, string> attributes, string grade, RegionalCareerPathRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int Version { get; set; } = version;
    public string Badge { get; set; } = badge;
    public string Country { get; set; } = country;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public RegionalCareerPathRollupStatus Status { get; set; } = status;

    /// <summary>Starts a regional career path rollup in status new.</summary>
    public static RegionalCareerPathRollup NewRegionalCareerPathRollup(string code, string title)
    {
        return new RegionalCareerPathRollup(
            code,
            title,
            "",
            0,
            "",
            "",
            "",
            0,
            new Dictionary<string, string>(),
            "",
            RegionalCareerPathRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalCareerPathRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalCareerPathRollupStatus next)
    {
        switch (Status)
        {
            case RegionalCareerPathRollupStatus.New:
                return next == RegionalCareerPathRollupStatus.Done;
            case RegionalCareerPathRollupStatus.Queued:
                return next == RegionalCareerPathRollupStatus.Running;
            case RegionalCareerPathRollupStatus.Running:
                return next == RegionalCareerPathRollupStatus.Done || next == RegionalCareerPathRollupStatus.Failed;
            case RegionalCareerPathRollupStatus.Done:
                return next == RegionalCareerPathRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalCareerPathRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional career path rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"regional career path rollup"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)Version + (double)TenureMonths, 2);
    }

    public RegionalCareerPathRollup Copy()
    {
        return new RegionalCareerPathRollup(
            Code,
            Title,
            Notes,
            Version,
            Badge,
            Country,
            Department,
            TenureMonths,
            Attributes,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
