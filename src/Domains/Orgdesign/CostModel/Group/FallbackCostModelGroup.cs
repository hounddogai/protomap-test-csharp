namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback cost model group is in its life cycle.</summary>
public enum FallbackCostModelGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback cost model group as the cost model team keeps it.</summary>
public sealed class FallbackCostModelGroup(string code, string title, double weight, int durationMinutes, int retries, bool approved, int quantity, int updatedAt, string ownerId, FallbackCostModelGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public int UpdatedAt { get; set; } = updatedAt;
    public string OwnerId { get; set; } = ownerId;
    public FallbackCostModelGroupStatus Status { get; set; } = status;

    /// <summary>Starts a fallback cost model group in status created.</summary>
    public static FallbackCostModelGroup NewFallbackCostModelGroup(string code, string title)
    {
        return new FallbackCostModelGroup(
            code,
            title,
            0.0,
            0,
            0,
            false,
            0,
            0,
            "",
            FallbackCostModelGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCostModelGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCostModelGroupStatus next)
    {
        switch (Status)
        {
            case FallbackCostModelGroupStatus.Created:
                switch (next)
                {
                    case FallbackCostModelGroupStatus.Validated:
                    case FallbackCostModelGroupStatus.Published:
                        return true;
                }
                return false;
            case FallbackCostModelGroupStatus.Validated:
                switch (next)
                {
                    case FallbackCostModelGroupStatus.Retired:
                        return true;
                }
                return false;
            case FallbackCostModelGroupStatus.Published:
                switch (next)
                {
                    case FallbackCostModelGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCostModelGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback cost model group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)DurationMinutes + (double)UpdatedAt, 2);
    }

    public FallbackCostModelGroup Copy()
    {
        return new FallbackCostModelGroup(
            Code,
            Title,
            Weight,
            DurationMinutes,
            Retries,
            Approved,
            Quantity,
            UpdatedAt,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
