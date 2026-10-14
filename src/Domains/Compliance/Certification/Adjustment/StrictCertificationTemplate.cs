namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict certification template is in its life cycle.</summary>
public enum StrictCertificationTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One strict certification template, with what the planners need to act on it.</summary>
public sealed class StrictCertificationTemplate(string code, string title, bool archived, string label, List<string> tags, int attempts, Dictionary<string, string> attributes, StrictCertificationTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public StrictCertificationTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a strict certification template in status created.</summary>
    public static StrictCertificationTemplate NewStrictCertificationTemplate(string code, string title)
    {
        return new StrictCertificationTemplate(
            code,
            title,
            false,
            "",
            new List<string>(),
            0,
            new Dictionary<string, string>(),
            StrictCertificationTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictCertificationTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictCertificationTemplateStatus next)
    {
        switch (Status)
        {
            case StrictCertificationTemplateStatus.Created:
                return next == StrictCertificationTemplateStatus.Validated || next == StrictCertificationTemplateStatus.Published;
            case StrictCertificationTemplateStatus.Validated:
                return next == StrictCertificationTemplateStatus.Published;
            case StrictCertificationTemplateStatus.Published:
                return next == StrictCertificationTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StrictCertificationTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict certification template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict certification template"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public StrictCertificationTemplate Copy()
    {
        return new StrictCertificationTemplate(Code, Title, Archived, Label, Tags, Attempts, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
