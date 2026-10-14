namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive rate template is in its life cycle.</summary>
public enum AdaptiveRateTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive rate template and its current status.</summary>
public sealed class AdaptiveRateTemplate(string code, string label, string phone, string channel, double rate, AdaptiveRateTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Channel { get; set; } = channel;
    public double Rate { get; set; } = rate;
    public AdaptiveRateTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive rate template in status idle.</summary>
    public static AdaptiveRateTemplate NewAdaptiveRateTemplate(string code, string label)
    {
        return new AdaptiveRateTemplate(code, label, "", "", 0.0, AdaptiveRateTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveRateTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveRateTemplateStatus next)
    {
        switch (Status)
        {
            case AdaptiveRateTemplateStatus.Idle:
                switch (next)
                {
                    case AdaptiveRateTemplateStatus.Warming:
                    case AdaptiveRateTemplateStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveRateTemplateStatus.Warming:
                return next == AdaptiveRateTemplateStatus.Ready;
            case AdaptiveRateTemplateStatus.Ready:
                return next == AdaptiveRateTemplateStatus.Draining;
            case AdaptiveRateTemplateStatus.Draining:
                return next == AdaptiveRateTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveRateTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive rate template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public AdaptiveRateTemplate Copy()
    {
        return new AdaptiveRateTemplate(Code, Label, Phone, Channel, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
