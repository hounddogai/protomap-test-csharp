namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback access badge override is in its life cycle.</summary>
public enum FallbackAccessBadgeOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback access badge override. Its code is unique within a tenant.</summary>
public sealed class FallbackAccessBadgeOverride(string code, string title, string checksum, string salaryBand, int expiresAt, int retries, string category, Dictionary<string, string> attributes, FallbackAccessBadgeOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public FallbackAccessBadgeOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a fallback access badge override in status new.</summary>
    public static FallbackAccessBadgeOverride NewFallbackAccessBadgeOverride(string code, string title)
    {
        return new FallbackAccessBadgeOverride(
            code,
            title,
            "",
            "",
            0,
            0,
            "",
            new Dictionary<string, string>(),
            FallbackAccessBadgeOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAccessBadgeOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAccessBadgeOverrideStatus next)
    {
        switch (Status)
        {
            case FallbackAccessBadgeOverrideStatus.New:
                switch (next)
                {
                    case FallbackAccessBadgeOverrideStatus.Failed:
                        return true;
                }
                return false;
            case FallbackAccessBadgeOverrideStatus.Queued:
                return next == FallbackAccessBadgeOverrideStatus.Running;
            case FallbackAccessBadgeOverrideStatus.Running:
                switch (next)
                {
                    case FallbackAccessBadgeOverrideStatus.Failed:
                        return true;
                }
                return false;
            case FallbackAccessBadgeOverrideStatus.Done:
                return next == FallbackAccessBadgeOverrideStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackAccessBadgeOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback access badge override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt - (double)Retries, 2);
    }

    public FallbackAccessBadgeOverride Copy()
    {
        return new FallbackAccessBadgeOverride(
            Code,
            Title,
            Checksum,
            SalaryBand,
            ExpiresAt,
            Retries,
            Category,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
