namespace Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback succession plan manifest is in its life cycle.</summary>
public enum FallbackSuccessionPlanManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback succession plan manifest. Its code is unique within a tenant.</summary>
public sealed class FallbackSuccessionPlanManifest(string code, string name, Dictionary<string, string> attributes, int version, List<string> tags, int windowDays, int updatedAt, double rating, FallbackSuccessionPlanManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public FallbackSuccessionPlanManifestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback succession plan manifest in status new.</summary>
    public static FallbackSuccessionPlanManifest NewFallbackSuccessionPlanManifest(string code, string name)
    {
        return new FallbackSuccessionPlanManifest(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            new List<string>(),
            0,
            0,
            0.0,
            FallbackSuccessionPlanManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSuccessionPlanManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSuccessionPlanManifestStatus next)
    {
        switch (Status)
        {
            case FallbackSuccessionPlanManifestStatus.New:
                switch (next)
                {
                    case FallbackSuccessionPlanManifestStatus.Failed:
                        return true;
                }
                return false;
            case FallbackSuccessionPlanManifestStatus.Queued:
                return next == FallbackSuccessionPlanManifestStatus.Done;
            case FallbackSuccessionPlanManifestStatus.Running:
                return next == FallbackSuccessionPlanManifestStatus.Done || next == FallbackSuccessionPlanManifestStatus.Failed;
            case FallbackSuccessionPlanManifestStatus.Done:
                switch (next)
                {
                    case FallbackSuccessionPlanManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSuccessionPlanManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback succession plan manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"fallback succession plan manifest"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt + Rating, 2);
    }

    public FallbackSuccessionPlanManifest Copy()
    {
        return new FallbackSuccessionPlanManifest(
            Code,
            Name,
            Attributes,
            Version,
            Tags,
            WindowDays,
            UpdatedAt,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
