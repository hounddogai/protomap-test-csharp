namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan quota is in its life cycle.</summary>
public enum SuccessionPlanQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a succession plan quota and its current status.</summary>
public sealed class SuccessionPlanQuota(string code, string title, int durationMinutes, string reviewer, int leaveDays, string department, double ceiling, string timezone, Dictionary<string, string> attributes, List<string> tags, SuccessionPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public int LeaveDays { get; set; } = leaveDays;
    public string Department { get; set; } = department;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public SuccessionPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan quota in status created.</summary>
    public static SuccessionPlanQuota NewSuccessionPlanQuota(string code, string title)
    {
        return new SuccessionPlanQuota(
            code,
            title,
            0,
            "",
            0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            new List<string>(),
            SuccessionPlanQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanQuotaStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanQuotaStatus.Created:
                switch (next)
                {
                    case SuccessionPlanQuotaStatus.Retired:
                        return true;
                }
                return false;
            case SuccessionPlanQuotaStatus.Validated:
                switch (next)
                {
                    case SuccessionPlanQuotaStatus.Published:
                    case SuccessionPlanQuotaStatus.Retired:
                        return true;
                }
                return false;
            case SuccessionPlanQuotaStatus.Published:
                switch (next)
                {
                    case SuccessionPlanQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)DurationMinutes + (double)LeaveDays, 2);
    }

    public SuccessionPlanQuota Copy()
    {
        return new SuccessionPlanQuota(
            Code,
            Title,
            DurationMinutes,
            Reviewer,
            LeaveDays,
            Department,
            Ceiling,
            Timezone,
            Attributes,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
