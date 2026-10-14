namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default access badge manifest is in its life cycle.</summary>
public enum DefaultAccessBadgeManifestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default access badge manifest. Its code is unique within a tenant.</summary>
public sealed class DefaultAccessBadgeManifest(string code, string label, Dictionary<string, string> attributes, string contactEmail, string segment, double hours, int durationMinutes, string description, string employeeId, string country, DefaultAccessBadgeManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public string Country { get; set; } = country;
    public DefaultAccessBadgeManifestStatus Status { get; set; } = status;

    /// <summary>Starts a default access badge manifest in status created.</summary>
    public static DefaultAccessBadgeManifest NewDefaultAccessBadgeManifest(string code, string label)
    {
        return new DefaultAccessBadgeManifest(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            0,
            "",
            "",
            "",
            DefaultAccessBadgeManifestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAccessBadgeManifestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAccessBadgeManifestStatus next)
    {
        switch (Status)
        {
            case DefaultAccessBadgeManifestStatus.Created:
                switch (next)
                {
                    case DefaultAccessBadgeManifestStatus.Published:
                        return true;
                }
                return false;
            case DefaultAccessBadgeManifestStatus.Validated:
                return next == DefaultAccessBadgeManifestStatus.Published;
            case DefaultAccessBadgeManifestStatus.Published:
                return next == DefaultAccessBadgeManifestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultAccessBadgeManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default access badge manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double WeightedValue()
    {
        return Math.Round(Hours + (double)DurationMinutes, 2);
    }

    public DefaultAccessBadgeManifest Copy()
    {
        return new DefaultAccessBadgeManifest(
            Code,
            Label,
            Attributes,
            ContactEmail,
            Segment,
            Hours,
            DurationMinutes,
            Description,
            EmployeeId,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
