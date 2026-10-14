namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band rule is in its life cycle.</summary>
public enum PayBandRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pay band rule, with what the planners need to act on it.</summary>
public sealed class PayBandRule(string code, string label, int revision, int sequence, double minimum, int durationMinutes, double score, string source, string contactEmail, PayBandRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Sequence { get; set; } = sequence;
    public double Minimum { get; set; } = minimum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public string ContactEmail { get; set; } = contactEmail;
    public PayBandRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pay band rule in status new.</summary>
    public static PayBandRule NewPayBandRule(string code, string label)
    {
        return new PayBandRule(code, label, 0, 0, 0.0, 0, 0.0, "", "", PayBandRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandRuleStatus next)
    {
        switch (Status)
        {
            case PayBandRuleStatus.New:
                return next == PayBandRuleStatus.Done;
            case PayBandRuleStatus.Queued:
                return next == PayBandRuleStatus.Done || next == PayBandRuleStatus.Failed;
            case PayBandRuleStatus.Running:
                switch (next)
                {
                    case PayBandRuleStatus.Done:
                        return true;
                }
                return false;
            case PayBandRuleStatus.Done:
                switch (next)
                {
                    case PayBandRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pay band rule"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision - (double)DurationMinutes, 2);
    }

    public PayBandRule Copy()
    {
        return new PayBandRule(
            Code,
            Label,
            Revision,
            Sequence,
            Minimum,
            DurationMinutes,
            Score,
            Source,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
