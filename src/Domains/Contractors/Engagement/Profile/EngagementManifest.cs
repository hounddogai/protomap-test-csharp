namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement manifest is in its life cycle.</summary>
public enum EngagementManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A engagement manifest as the engagement team keeps it.</summary>
public sealed class EngagementManifest(string code, string title, Dictionary<string, string> attributes, string channel, string grade, double score, EngagementManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public double Score { get; set; } = score;
    public EngagementManifestStatus Status { get; set; } = status;

    /// <summary>Starts a engagement manifest in status created.</summary>
    public static EngagementManifest NewEngagementManifest(string code, string title)
    {
        return new EngagementManifest(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            EngagementManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementManifestStatus next)
    {
        switch (Status)
        {
            case EngagementManifestStatus.Created:
                return next == EngagementManifestStatus.Validated;
            case EngagementManifestStatus.Validated:
                switch (next)
                {
                    case EngagementManifestStatus.Published:
                        return true;
                }
                return false;
            case EngagementManifestStatus.Published:
                return next == EngagementManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(EngagementManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public EngagementManifest Copy()
    {
        return new EngagementManifest(Code, Title, Attributes, Channel, Grade, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
