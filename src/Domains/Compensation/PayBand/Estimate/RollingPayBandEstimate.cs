namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pay band estimate is in its life cycle.</summary>
public enum RollingPayBandEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling pay band estimate. Its code is unique within a tenant.</summary>
public sealed class RollingPayBandEstimate(string code, string title, double fte, int createdAt, string department, double score, double weight, RollingPayBandEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int CreatedAt { get; set; } = createdAt;
    public string Department { get; set; } = department;
    public double Score { get; set; } = score;
    public double Weight { get; set; } = weight;
    public RollingPayBandEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pay band estimate in status draft.</summary>
    public static RollingPayBandEstimate NewRollingPayBandEstimate(string code, string title)
    {
        return new RollingPayBandEstimate(code, title, 0.0, 0, "", 0.0, 0.0, RollingPayBandEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayBandEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayBandEstimateStatus next)
    {
        switch (Status)
        {
            case RollingPayBandEstimateStatus.Draft:
                return next == RollingPayBandEstimateStatus.Pending || next == RollingPayBandEstimateStatus.Active;
            case RollingPayBandEstimateStatus.Pending:
                switch (next)
                {
                    case RollingPayBandEstimateStatus.Active:
                    case RollingPayBandEstimateStatus.Closed:
                        return true;
                }
                return false;
            case RollingPayBandEstimateStatus.Active:
                return next == RollingPayBandEstimateStatus.Suspended || next == RollingPayBandEstimateStatus.Closed;
            case RollingPayBandEstimateStatus.Suspended:
                switch (next)
                {
                    case RollingPayBandEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPayBandEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pay band estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling pay band estimate"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight * Fte, 2);
    }

    public RollingPayBandEstimate Copy()
    {
        return new RollingPayBandEstimate(Code, Title, Fte, CreatedAt, Department, Score, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
