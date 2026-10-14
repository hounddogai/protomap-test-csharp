namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual estimate is in its life cycle.</summary>
public enum AccrualEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a accrual estimate and its current status.</summary>
public sealed class AccrualEstimate(string code, string name, int windowDays, double floorValue, double score, AccrualEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public double FloorValue { get; set; } = floorValue;
    public double Score { get; set; } = score;
    public AccrualEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a accrual estimate in status draft.</summary>
    public static AccrualEstimate NewAccrualEstimate(string code, string name)
    {
        return new AccrualEstimate(code, name, 0, 0.0, 0.0, AccrualEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualEstimateStatus next)
    {
        switch (Status)
        {
            case AccrualEstimateStatus.Draft:
                switch (next)
                {
                    case AccrualEstimateStatus.Pending:
                        return true;
                }
                return false;
            case AccrualEstimateStatus.Pending:
                return next == AccrualEstimateStatus.Active || next == AccrualEstimateStatus.Closed;
            case AccrualEstimateStatus.Active:
                return next == AccrualEstimateStatus.Suspended || next == AccrualEstimateStatus.Closed;
            case AccrualEstimateStatus.Suspended:
                switch (next)
                {
                    case AccrualEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays - FloorValue, 2);
    }

    public AccrualEstimate Copy()
    {
        return new AccrualEstimate(Code, Name, WindowDays, FloorValue, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
