namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model milestone is in its life cycle.</summary>
public enum CostModelMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a cost model milestone and its current status.</summary>
public sealed class CostModelMilestone(string code, string label, string segment, double score, bool archived, double rating, int updatedAt, int endsAt, string ownerId, CostModelMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public string OwnerId { get; set; } = ownerId;
    public CostModelMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a cost model milestone in status created.</summary>
    public static CostModelMilestone NewCostModelMilestone(string code, string label)
    {
        return new CostModelMilestone(code, label, "", 0.0, false, 0.0, 0, 0, "", CostModelMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelMilestoneStatus next)
    {
        switch (Status)
        {
            case CostModelMilestoneStatus.Created:
                switch (next)
                {
                    case CostModelMilestoneStatus.Validated:
                        return true;
                }
                return false;
            case CostModelMilestoneStatus.Validated:
                switch (next)
                {
                    case CostModelMilestoneStatus.Retired:
                        return true;
                }
                return false;
            case CostModelMilestoneStatus.Published:
                return next == CostModelMilestoneStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CostModelMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)UpdatedAt - Score, 2);
    }

    public CostModelMilestone Copy()
    {
        return new CostModelMilestone(
            Code,
            Label,
            Segment,
            Score,
            Archived,
            Rating,
            UpdatedAt,
            EndsAt,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
