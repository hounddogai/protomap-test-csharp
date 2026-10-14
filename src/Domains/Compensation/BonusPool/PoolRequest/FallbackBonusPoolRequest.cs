namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback bonus pool request is in its life cycle.</summary>
public enum FallbackBonusPoolRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback bonus pool request and its current status.</summary>
public sealed class FallbackBonusPoolRequest(string code, string name, string department, double rating, double margin, string label, int quantity, int attempts, string segment, int priority, FallbackBonusPoolRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public double Rating { get; set; } = rating;
    public double Margin { get; set; } = margin;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int Attempts { get; set; } = attempts;
    public string Segment { get; set; } = segment;
    public int Priority { get; set; } = priority;
    public FallbackBonusPoolRequestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback bonus pool request in status idle.</summary>
    public static FallbackBonusPoolRequest NewFallbackBonusPoolRequest(string code, string name)
    {
        return new FallbackBonusPoolRequest(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            0,
            0,
            "",
            0,
            FallbackBonusPoolRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBonusPoolRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBonusPoolRequestStatus next)
    {
        switch (Status)
        {
            case FallbackBonusPoolRequestStatus.Idle:
                switch (next)
                {
                    case FallbackBonusPoolRequestStatus.Draining:
                        return true;
                }
                return false;
            case FallbackBonusPoolRequestStatus.Warming:
                return next == FallbackBonusPoolRequestStatus.Stopped;
            case FallbackBonusPoolRequestStatus.Ready:
                return next == FallbackBonusPoolRequestStatus.Draining;
            case FallbackBonusPoolRequestStatus.Draining:
                return next == FallbackBonusPoolRequestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackBonusPoolRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback bonus pool request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Quantity - (double)Priority, 2);
    }

    public FallbackBonusPoolRequest Copy()
    {
        return new FallbackBonusPoolRequest(
            Code,
            Name,
            Department,
            Rating,
            Margin,
            Label,
            Quantity,
            Attempts,
            Segment,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
