namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional skill profile manifest is in its life cycle.</summary>
public enum RegionalSkillProfileManifestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional skill profile manifest as the skill profile team keeps it.</summary>
public sealed class RegionalSkillProfileManifest(string code, string name, string notes, string checksum, double margin, Dictionary<string, string> attributes, double budget, int windowDays, int attempts, string summary, RegionalSkillProfileManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Checksum { get; set; } = checksum;
    public double Margin { get; set; } = margin;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public int WindowDays { get; set; } = windowDays;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public RegionalSkillProfileManifestStatus Status { get; set; } = status;

    /// <summary>Starts a regional skill profile manifest in status idle.</summary>
    public static RegionalSkillProfileManifest NewRegionalSkillProfileManifest(string code, string name)
    {
        return new RegionalSkillProfileManifest(
            code,
            name,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0,
            0,
            "",
            RegionalSkillProfileManifestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalSkillProfileManifestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalSkillProfileManifestStatus next)
    {
        switch (Status)
        {
            case RegionalSkillProfileManifestStatus.Idle:
                switch (next)
                {
                    case RegionalSkillProfileManifestStatus.Ready:
                        return true;
                }
                return false;
            case RegionalSkillProfileManifestStatus.Warming:
                return next == RegionalSkillProfileManifestStatus.Draining || next == RegionalSkillProfileManifestStatus.Stopped;
            case RegionalSkillProfileManifestStatus.Ready:
                return next == RegionalSkillProfileManifestStatus.Stopped;
            case RegionalSkillProfileManifestStatus.Draining:
                return next == RegionalSkillProfileManifestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalSkillProfileManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional skill profile manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts * Budget, 2);
    }

    public RegionalSkillProfileManifest Copy()
    {
        return new RegionalSkillProfileManifest(
            Code,
            Name,
            Notes,
            Checksum,
            Margin,
            Attributes,
            Budget,
            WindowDays,
            Attempts,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
