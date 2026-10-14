namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled payslip notice is in its life cycle.</summary>
public enum ScheduledPayslipNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled payslip notice and its current status.</summary>
public sealed class ScheduledPayslipNotice(string code, string name, string label, double ratio, List<string> tags, int revision, int windowDays, Dictionary<string, string> attributes, string source, double rate, ScheduledPayslipNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public int Revision { get; set; } = revision;
    public int WindowDays { get; set; } = windowDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public ScheduledPayslipNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled payslip notice in status idle.</summary>
    public static ScheduledPayslipNotice NewScheduledPayslipNotice(string code, string name)
    {
        return new ScheduledPayslipNotice(
            code,
            name,
            "",
            0.0,
            new List<string>(),
            0,
            0,
            new Dictionary<string, string>(),
            "",
            0.0,
            ScheduledPayslipNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayslipNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayslipNoticeStatus next)
    {
        switch (Status)
        {
            case ScheduledPayslipNoticeStatus.Idle:
                switch (next)
                {
                    case ScheduledPayslipNoticeStatus.Ready:
                    case ScheduledPayslipNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPayslipNoticeStatus.Warming:
                switch (next)
                {
                    case ScheduledPayslipNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPayslipNoticeStatus.Ready:
                return next == ScheduledPayslipNoticeStatus.Stopped;
            case ScheduledPayslipNoticeStatus.Draining:
                switch (next)
                {
                    case ScheduledPayslipNoticeStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayslipNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled payslip notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
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

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)WindowDays + Ratio, 2);
    }

    public ScheduledPayslipNotice Copy()
    {
        return new ScheduledPayslipNotice(
            Code,
            Name,
            Label,
            Ratio,
            Tags,
            Revision,
            WindowDays,
            Attributes,
            Source,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
