namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan allocation is in its life cycle.</summary>
public enum ReturnPlanAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A return plan allocation. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanAllocation(string code, string name, double rate, string grade, string locale, string costCenter, string jobCode, double minimum, string reviewer, ReturnPlanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public string JobCode { get; set; } = jobCode;
    public double Minimum { get; set; } = minimum;
    public string Reviewer { get; set; } = reviewer;
    public ReturnPlanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a return plan allocation in status new.</summary>
    public static ReturnPlanAllocation NewReturnPlanAllocation(string code, string name)
    {
        return new ReturnPlanAllocation(code, name, 0.0, "", "", "", "", 0.0, "", ReturnPlanAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanAllocationStatus next)
    {
        switch (Status)
        {
            case ReturnPlanAllocationStatus.New:
                switch (next)
                {
                    case ReturnPlanAllocationStatus.Running:
                        return true;
                }
                return false;
            case ReturnPlanAllocationStatus.Queued:
                return next == ReturnPlanAllocationStatus.Done || next == ReturnPlanAllocationStatus.Failed;
            case ReturnPlanAllocationStatus.Running:
                switch (next)
                {
                    case ReturnPlanAllocationStatus.Done:
                        return true;
                }
                return false;
            case ReturnPlanAllocationStatus.Done:
                switch (next)
                {
                    case ReturnPlanAllocationStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"return plan allocation"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Minimum - Rate, 2);
    }

    public ReturnPlanAllocation Copy()
    {
        return new ReturnPlanAllocation(
            Code,
            Name,
            Rate,
            Grade,
            Locale,
            CostCenter,
            JobCode,
            Minimum,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
