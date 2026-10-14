namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted cost model manifest is in its life cycle.</summary>
public enum WeightedCostModelManifestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One weighted cost model manifest, with what the planners need to act on it.</summary>
public sealed class WeightedCostModelManifest(string code, string name, double threshold, double rating, int updatedAt, int durationMinutes, bool active, string team, string title, int quantity, WeightedCostModelManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Active { get; set; } = active;
    public string Team { get; set; } = team;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public WeightedCostModelManifestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted cost model manifest in status proposed.</summary>
    public static WeightedCostModelManifest NewWeightedCostModelManifest(string code, string name)
    {
        return new WeightedCostModelManifest(
            code,
            name,
            0.0,
            0.0,
            0,
            0,
            false,
            "",
            "",
            0,
            WeightedCostModelManifestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedCostModelManifestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedCostModelManifestStatus next)
    {
        switch (Status)
        {
            case WeightedCostModelManifestStatus.Proposed:
                return next == WeightedCostModelManifestStatus.Withdrawn;
            case WeightedCostModelManifestStatus.Approved:
                switch (next)
                {
                    case WeightedCostModelManifestStatus.Rejected:
                    case WeightedCostModelManifestStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedCostModelManifestStatus.Rejected:
                return next == WeightedCostModelManifestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedCostModelManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted cost model manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted cost model manifest"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold * (double)UpdatedAt, 2);
    }

    public WeightedCostModelManifest Copy()
    {
        return new WeightedCostModelManifest(
            Code,
            Name,
            Threshold,
            Rating,
            UpdatedAt,
            DurationMinutes,
            Active,
            Team,
            Title,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
