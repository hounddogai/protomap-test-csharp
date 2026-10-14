namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback cost model request is in its life cycle.</summary>
public enum FallbackCostModelRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a fallback cost model request and its current status.</summary>
public sealed class FallbackCostModelRequest(string code, string label, int updatedAt, double ratio, List<string> tags, double score, double hours, string segment, string contactEmail, double rating, double cost, FallbackCostModelRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public double Cost { get; set; } = cost;
    public FallbackCostModelRequestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback cost model request in status new.</summary>
    public static FallbackCostModelRequest NewFallbackCostModelRequest(string code, string label)
    {
        return new FallbackCostModelRequest(
            code,
            label,
            0,
            0.0,
            new List<string>(),
            0.0,
            0.0,
            "",
            "",
            0.0,
            0.0,
            FallbackCostModelRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCostModelRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCostModelRequestStatus next)
    {
        switch (Status)
        {
            case FallbackCostModelRequestStatus.New:
                switch (next)
                {
                    case FallbackCostModelRequestStatus.Running:
                        return true;
                }
                return false;
            case FallbackCostModelRequestStatus.Queued:
                switch (next)
                {
                    case FallbackCostModelRequestStatus.Done:
                        return true;
                }
                return false;
            case FallbackCostModelRequestStatus.Running:
                switch (next)
                {
                    case FallbackCostModelRequestStatus.Done:
                    case FallbackCostModelRequestStatus.Failed:
                        return true;
                }
                return false;
            case FallbackCostModelRequestStatus.Done:
                switch (next)
                {
                    case FallbackCostModelRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackCostModelRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback cost model request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double AdjustedVolume()
    {
        return Math.Round(Cost - Ratio, 2);
    }

    public FallbackCostModelRequest Copy()
    {
        return new FallbackCostModelRequest(
            Code,
            Label,
            UpdatedAt,
            Ratio,
            Tags,
            Score,
            Hours,
            Segment,
            ContactEmail,
            Rating,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
