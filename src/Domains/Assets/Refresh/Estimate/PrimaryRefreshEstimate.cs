namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary refresh estimate is in its life cycle.</summary>
public enum PrimaryRefreshEstimateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One primary refresh estimate, with what the planners need to act on it.</summary>
public sealed class PrimaryRefreshEstimate(string code, string label, string badge, double score, double amount, double fte, int batchSize, int endsAt, bool visible, PrimaryRefreshEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public PrimaryRefreshEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a primary refresh estimate in status requested.</summary>
    public static PrimaryRefreshEstimate NewPrimaryRefreshEstimate(string code, string label)
    {
        return new PrimaryRefreshEstimate(
            code,
            label,
            "",
            0.0,
            0.0,
            0.0,
            0,
            0,
            false,
            PrimaryRefreshEstimateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRefreshEstimateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRefreshEstimateStatus next)
    {
        switch (Status)
        {
            case PrimaryRefreshEstimateStatus.Requested:
                switch (next)
                {
                    case PrimaryRefreshEstimateStatus.Revoked:
                    case PrimaryRefreshEstimateStatus.Expired:
                        return true;
                }
                return false;
            case PrimaryRefreshEstimateStatus.Granted:
                return next == PrimaryRefreshEstimateStatus.Revoked;
            case PrimaryRefreshEstimateStatus.Revoked:
                return next == PrimaryRefreshEstimateStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PrimaryRefreshEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary refresh estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount - (double)BatchSize, 2);
    }

    public PrimaryRefreshEstimate Copy()
    {
        return new PrimaryRefreshEstimate(
            Code,
            Label,
            Badge,
            Score,
            Amount,
            Fte,
            BatchSize,
            EndsAt,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
