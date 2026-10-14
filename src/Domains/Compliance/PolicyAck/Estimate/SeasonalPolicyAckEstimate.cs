namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal policy ack estimate is in its life cycle.</summary>
public enum SeasonalPolicyAckEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One seasonal policy ack estimate, with what the planners need to act on it.</summary>
public sealed class SeasonalPolicyAckEstimate(string code, string label, string phone, bool active, string costCenter, double ceiling, double discount, string reviewer, int sequence, double hours, int endsAt, SeasonalPolicyAckEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public int EndsAt { get; set; } = endsAt;
    public SeasonalPolicyAckEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal policy ack estimate in status planned.</summary>
    public static SeasonalPolicyAckEstimate NewSeasonalPolicyAckEstimate(string code, string label)
    {
        return new SeasonalPolicyAckEstimate(
            code,
            label,
            "",
            false,
            "",
            0.0,
            0.0,
            "",
            0,
            0.0,
            0,
            SeasonalPolicyAckEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPolicyAckEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPolicyAckEstimateStatus next)
    {
        switch (Status)
        {
            case SeasonalPolicyAckEstimateStatus.Planned:
                return next == SeasonalPolicyAckEstimateStatus.Scheduled;
            case SeasonalPolicyAckEstimateStatus.Scheduled:
                return next == SeasonalPolicyAckEstimateStatus.Completed;
            case SeasonalPolicyAckEstimateStatus.InProgress:
                switch (next)
                {
                    case SeasonalPolicyAckEstimateStatus.Completed:
                    case SeasonalPolicyAckEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalPolicyAckEstimateStatus.Completed:
                return next == SeasonalPolicyAckEstimateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalPolicyAckEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal policy ack estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Sequence + (double)EndsAt, 2);
    }

    public SeasonalPolicyAckEstimate Copy()
    {
        return new SeasonalPolicyAckEstimate(
            Code,
            Label,
            Phone,
            Active,
            CostCenter,
            Ceiling,
            Discount,
            Reviewer,
            Sequence,
            Hours,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
