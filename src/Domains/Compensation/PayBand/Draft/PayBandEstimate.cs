namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band estimate is in its life cycle.</summary>
public enum PayBandEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pay band estimate, with what the planners need to act on it.</summary>
public sealed class PayBandEstimate(string code, string label, int sequence, string source, int endsAt, int durationMinutes, string ownerId, PayBandEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Source { get; set; } = source;
    public int EndsAt { get; set; } = endsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public PayBandEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pay band estimate in status new.</summary>
    public static PayBandEstimate NewPayBandEstimate(string code, string label)
    {
        return new PayBandEstimate(code, label, 0, "", 0, 0, "", PayBandEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandEstimateStatus next)
    {
        switch (Status)
        {
            case PayBandEstimateStatus.New:
                switch (next)
                {
                    case PayBandEstimateStatus.Running:
                        return true;
                }
                return false;
            case PayBandEstimateStatus.Queued:
                return next == PayBandEstimateStatus.Done || next == PayBandEstimateStatus.Failed;
            case PayBandEstimateStatus.Running:
                switch (next)
                {
                    case PayBandEstimateStatus.Done:
                    case PayBandEstimateStatus.Failed:
                        return true;
                }
                return false;
            case PayBandEstimateStatus.Done:
                switch (next)
                {
                    case PayBandEstimateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)EndsAt * (double)Sequence, 2);
    }

    public PayBandEstimate Copy()
    {
        return new PayBandEstimate(Code, Label, Sequence, Source, EndsAt, DurationMinutes, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
