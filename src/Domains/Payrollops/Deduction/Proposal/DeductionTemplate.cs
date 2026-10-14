namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction template is in its life cycle.</summary>
public enum DeductionTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One deduction template, with what the planners need to act on it.</summary>
public sealed class DeductionTemplate(string code, string label, double threshold, string grade, double cost, DeductionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public DeductionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a deduction template in status idle.</summary>
    public static DeductionTemplate NewDeductionTemplate(string code, string label)
    {
        return new DeductionTemplate(code, label, 0.0, "", 0.0, DeductionTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionTemplateStatus next)
    {
        switch (Status)
        {
            case DeductionTemplateStatus.Idle:
                return next == DeductionTemplateStatus.Draining || next == DeductionTemplateStatus.Stopped;
            case DeductionTemplateStatus.Warming:
                return next == DeductionTemplateStatus.Stopped;
            case DeductionTemplateStatus.Ready:
                return next == DeductionTemplateStatus.Stopped;
            case DeductionTemplateStatus.Draining:
                switch (next)
                {
                    case DeductionTemplateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Threshold - Cost, 2);
    }

    public DeductionTemplate Copy()
    {
        return new DeductionTemplate(Code, Label, Threshold, Grade, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
