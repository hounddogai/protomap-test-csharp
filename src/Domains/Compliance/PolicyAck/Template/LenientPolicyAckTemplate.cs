namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy ack template is in its life cycle.</summary>
public enum LenientPolicyAckTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a lenient policy ack template and its current status.</summary>
public sealed class LenientPolicyAckTemplate(string code, string title, int durationMinutes, string name, string externalRef, double threshold, int sequence, LenientPolicyAckTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public int Sequence { get; set; } = sequence;
    public LenientPolicyAckTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy ack template in status idle.</summary>
    public static LenientPolicyAckTemplate NewLenientPolicyAckTemplate(string code, string title)
    {
        return new LenientPolicyAckTemplate(code, title, 0, "", "", 0.0, 0, LenientPolicyAckTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyAckTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyAckTemplateStatus next)
    {
        switch (Status)
        {
            case LenientPolicyAckTemplateStatus.Idle:
                switch (next)
                {
                    case LenientPolicyAckTemplateStatus.Ready:
                    case LenientPolicyAckTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case LenientPolicyAckTemplateStatus.Warming:
                return next == LenientPolicyAckTemplateStatus.Ready;
            case LenientPolicyAckTemplateStatus.Ready:
                return next == LenientPolicyAckTemplateStatus.Stopped;
            case LenientPolicyAckTemplateStatus.Draining:
                return next == LenientPolicyAckTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientPolicyAckTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy ack template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient policy ack template"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold * (double)DurationMinutes, 2);
    }

    public LenientPolicyAckTemplate Copy()
    {
        return new LenientPolicyAckTemplate(
            Code,
            Title,
            DurationMinutes,
            Name,
            ExternalRef,
            Threshold,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
