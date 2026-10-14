namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted access badge batch is in its life cycle.</summary>
public enum WeightedAccessBadgeBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted access badge batch, with what the planners need to act on it.</summary>
public sealed class WeightedAccessBadgeBatch(string code, string label, bool archived, int graceDays, string checksum, int endsAt, string title, double weight, int capacity, WeightedAccessBadgeBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int GraceDays { get; set; } = graceDays;
    public string Checksum { get; set; } = checksum;
    public int EndsAt { get; set; } = endsAt;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public WeightedAccessBadgeBatchStatus Status { get; set; } = status;

    /// <summary>Starts a weighted access badge batch in status open.</summary>
    public static WeightedAccessBadgeBatch NewWeightedAccessBadgeBatch(string code, string label)
    {
        return new WeightedAccessBadgeBatch(
            code,
            label,
            false,
            0,
            "",
            0,
            "",
            0.0,
            0,
            WeightedAccessBadgeBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccessBadgeBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccessBadgeBatchStatus next)
    {
        switch (Status)
        {
            case WeightedAccessBadgeBatchStatus.Open:
                switch (next)
                {
                    case WeightedAccessBadgeBatchStatus.InReview:
                    case WeightedAccessBadgeBatchStatus.Resolved:
                        return true;
                }
                return false;
            case WeightedAccessBadgeBatchStatus.InReview:
                return next == WeightedAccessBadgeBatchStatus.Archived;
            case WeightedAccessBadgeBatchStatus.Resolved:
                return next == WeightedAccessBadgeBatchStatus.Archived;
            case WeightedAccessBadgeBatchStatus.Reopened:
                return next == WeightedAccessBadgeBatchStatus.Archived;
        }
        return false;
    }

    public void MoveTo(WeightedAccessBadgeBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted access badge batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"weighted access badge batch"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Weight + (double)GraceDays, 2);
    }

    public WeightedAccessBadgeBatch Copy()
    {
        return new WeightedAccessBadgeBatch(
            Code,
            Label,
            Archived,
            GraceDays,
            Checksum,
            EndsAt,
            Title,
            Weight,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
