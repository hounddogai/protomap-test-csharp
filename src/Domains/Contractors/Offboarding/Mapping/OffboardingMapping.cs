namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding mapping is in its life cycle.</summary>
public enum OffboardingMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A offboarding mapping. Its code is unique within a tenant.</summary>
public sealed class OffboardingMapping(string code, string title, int batchSize, string category, string locale, double ratio, double maximum, string ownerId, int attempts, OffboardingMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public double Ratio { get; set; } = ratio;
    public double Maximum { get; set; } = maximum;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public OffboardingMappingStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding mapping in status open.</summary>
    public static OffboardingMapping NewOffboardingMapping(string code, string title)
    {
        return new OffboardingMapping(code, title, 0, "", "", 0.0, 0.0, "", 0, OffboardingMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingMappingStatus next)
    {
        switch (Status)
        {
            case OffboardingMappingStatus.Open:
                return next == OffboardingMappingStatus.Archived;
            case OffboardingMappingStatus.InReview:
                return next == OffboardingMappingStatus.Reopened;
            case OffboardingMappingStatus.Resolved:
                switch (next)
                {
                    case OffboardingMappingStatus.Reopened:
                        return true;
                }
                return false;
            case OffboardingMappingStatus.Reopened:
                return next == OffboardingMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OffboardingMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding mapping"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize * Maximum, 2);
    }

    public OffboardingMapping Copy()
    {
        return new OffboardingMapping(
            Code,
            Title,
            BatchSize,
            Category,
            Locale,
            Ratio,
            Maximum,
            OwnerId,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
