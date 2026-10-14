namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan target is in its life cycle.</summary>
public enum ReturnPlanTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a return plan target and its current status.</summary>
public sealed class ReturnPlanTarget(string code, string label, double margin, double floorValue, string country, double rate, int sequence, double ceiling, string category, int attempts, string unit, ReturnPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public int Sequence { get; set; } = sequence;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public string Unit { get; set; } = unit;
    public ReturnPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a return plan target in status proposed.</summary>
    public static ReturnPlanTarget NewReturnPlanTarget(string code, string label)
    {
        return new ReturnPlanTarget(
            code,
            label,
            0.0,
            0.0,
            "",
            0.0,
            0,
            0.0,
            "",
            0,
            "",
            ReturnPlanTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanTargetStatus next)
    {
        switch (Status)
        {
            case ReturnPlanTargetStatus.Proposed:
                return next == ReturnPlanTargetStatus.Withdrawn;
            case ReturnPlanTargetStatus.Approved:
                switch (next)
                {
                    case ReturnPlanTargetStatus.Rejected:
                    case ReturnPlanTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case ReturnPlanTargetStatus.Rejected:
                switch (next)
                {
                    case ReturnPlanTargetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue + (double)Attempts, 2);
    }

    public ReturnPlanTarget Copy()
    {
        return new ReturnPlanTarget(
            Code,
            Label,
            Margin,
            FloorValue,
            Country,
            Rate,
            Sequence,
            Ceiling,
            Category,
            Attempts,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
