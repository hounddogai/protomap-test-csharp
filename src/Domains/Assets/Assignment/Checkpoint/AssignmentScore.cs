namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment score is in its life cycle.</summary>
public enum AssignmentScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One assignment score, with what the planners need to act on it.</summary>
public sealed class AssignmentScore(string code, string title, string country, double cost, double floorValue, string externalRef, int updatedAt, double ratio, double score, AssignmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public double FloorValue { get; set; } = floorValue;
    public string ExternalRef { get; set; } = externalRef;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public AssignmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a assignment score in status open.</summary>
    public static AssignmentScore NewAssignmentScore(string code, string title)
    {
        return new AssignmentScore(code, title, "", 0.0, 0.0, "", 0, 0.0, 0.0, AssignmentScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentScoreStatus next)
    {
        switch (Status)
        {
            case AssignmentScoreStatus.Open:
                return next == AssignmentScoreStatus.InReview || next == AssignmentScoreStatus.Reopened;
            case AssignmentScoreStatus.InReview:
                switch (next)
                {
                    case AssignmentScoreStatus.Resolved:
                        return true;
                }
                return false;
            case AssignmentScoreStatus.Resolved:
                switch (next)
                {
                    case AssignmentScoreStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentScoreStatus.Reopened:
                return next == AssignmentScoreStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssignmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(FloorValue + Score, 2);
    }

    public AssignmentScore Copy()
    {
        return new AssignmentScore(
            Code,
            Title,
            Country,
            Cost,
            FloorValue,
            ExternalRef,
            UpdatedAt,
            Ratio,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
