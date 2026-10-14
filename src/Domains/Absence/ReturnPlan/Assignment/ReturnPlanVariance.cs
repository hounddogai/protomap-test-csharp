namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan variance is in its life cycle.</summary>
public enum ReturnPlanVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A return plan variance. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanVariance(string code, string name, double maximum, double floorValue, string reviewer, ReturnPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public string Reviewer { get; set; } = reviewer;
    public ReturnPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a return plan variance in status requested.</summary>
    public static ReturnPlanVariance NewReturnPlanVariance(string code, string name)
    {
        return new ReturnPlanVariance(code, name, 0.0, 0.0, "", ReturnPlanVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanVarianceStatus next)
    {
        switch (Status)
        {
            case ReturnPlanVarianceStatus.Requested:
                switch (next)
                {
                    case ReturnPlanVarianceStatus.Granted:
                    case ReturnPlanVarianceStatus.Expired:
                        return true;
                }
                return false;
            case ReturnPlanVarianceStatus.Granted:
                return next == ReturnPlanVarianceStatus.Revoked;
            case ReturnPlanVarianceStatus.Revoked:
                return next == ReturnPlanVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum - FloorValue, 2);
    }

    public ReturnPlanVariance Copy()
    {
        return new ReturnPlanVariance(Code, Name, Maximum, FloorValue, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
