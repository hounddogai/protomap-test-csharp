namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile manifest is in its life cycle.</summary>
public enum SkillProfileManifestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a skill profile manifest and its current status.</summary>
public sealed class SkillProfileManifest(string code, string label, string team, double ratio, List<string> tags, double weight, string ownerId, string name, int revision, int limit, string country, SkillProfileManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public double Weight { get; set; } = weight;
    public string OwnerId { get; set; } = ownerId;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public int Limit { get; set; } = limit;
    public string Country { get; set; } = country;
    public SkillProfileManifestStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile manifest in status requested.</summary>
    public static SkillProfileManifest NewSkillProfileManifest(string code, string label)
    {
        return new SkillProfileManifest(
            code,
            label,
            "",
            0.0,
            new List<string>(),
            0.0,
            "",
            "",
            0,
            0,
            "",
            SkillProfileManifestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileManifestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileManifestStatus next)
    {
        switch (Status)
        {
            case SkillProfileManifestStatus.Requested:
                switch (next)
                {
                    case SkillProfileManifestStatus.Expired:
                        return true;
                }
                return false;
            case SkillProfileManifestStatus.Granted:
                switch (next)
                {
                    case SkillProfileManifestStatus.Revoked:
                        return true;
                }
                return false;
            case SkillProfileManifestStatus.Revoked:
                return next == SkillProfileManifestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SkillProfileManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double NetVolume()
    {
        return Math.Round((double)Limit + (double)Revision, 2);
    }

    public SkillProfileManifest Copy()
    {
        return new SkillProfileManifest(
            Code,
            Label,
            Team,
            Ratio,
            Tags,
            Weight,
            OwnerId,
            Name,
            Revision,
            Limit,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
