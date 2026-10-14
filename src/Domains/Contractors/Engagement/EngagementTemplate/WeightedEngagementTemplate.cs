namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted engagement template is in its life cycle.</summary>
public enum WeightedEngagementTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a weighted engagement template and its current status.</summary>
public sealed class WeightedEngagementTemplate(string code, string title, double threshold, int tenureMonths, int attempts, List<string> tags, bool approved, string description, bool archived, double minimum, double ceiling, WeightedEngagementTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Attempts { get; set; } = attempts;
    public List<string> Tags { get; set; } = tags;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public double Ceiling { get; set; } = ceiling;
    public WeightedEngagementTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a weighted engagement template in status requested.</summary>
    public static WeightedEngagementTemplate NewWeightedEngagementTemplate(string code, string title)
    {
        return new WeightedEngagementTemplate(
            code,
            title,
            0.0,
            0,
            0,
            new List<string>(),
            false,
            "",
            false,
            0.0,
            0.0,
            WeightedEngagementTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedEngagementTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedEngagementTemplateStatus next)
    {
        switch (Status)
        {
            case WeightedEngagementTemplateStatus.Requested:
                switch (next)
                {
                    case WeightedEngagementTemplateStatus.Granted:
                    case WeightedEngagementTemplateStatus.Expired:
                        return true;
                }
                return false;
            case WeightedEngagementTemplateStatus.Granted:
                switch (next)
                {
                    case WeightedEngagementTemplateStatus.Revoked:
                        return true;
                }
                return false;
            case WeightedEngagementTemplateStatus.Revoked:
                switch (next)
                {
                    case WeightedEngagementTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedEngagementTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted engagement template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted engagement template"} {Code}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Minimum - (double)TenureMonths, 2);
    }

    public WeightedEngagementTemplate Copy()
    {
        return new WeightedEngagementTemplate(
            Code,
            Title,
            Threshold,
            TenureMonths,
            Attempts,
            Tags,
            Approved,
            Description,
            Archived,
            Minimum,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
