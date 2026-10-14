namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict accrual exception is in its life cycle.</summary>
public enum StrictAccrualExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict accrual exception and its current status.</summary>
public sealed class StrictAccrualException(string code, string label, int windowDays, string timezone, double ratio, StrictAccrualExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public StrictAccrualExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a strict accrual exception in status open.</summary>
    public static StrictAccrualException NewStrictAccrualException(string code, string label)
    {
        return new StrictAccrualException(code, label, 0, "", 0.0, StrictAccrualExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccrualExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccrualExceptionStatus next)
    {
        switch (Status)
        {
            case StrictAccrualExceptionStatus.Open:
                return next == StrictAccrualExceptionStatus.InReview || next == StrictAccrualExceptionStatus.Archived;
            case StrictAccrualExceptionStatus.InReview:
                switch (next)
                {
                    case StrictAccrualExceptionStatus.Reopened:
                    case StrictAccrualExceptionStatus.Archived:
                        return true;
                }
                return false;
            case StrictAccrualExceptionStatus.Resolved:
                switch (next)
                {
                    case StrictAccrualExceptionStatus.Archived:
                        return true;
                }
                return false;
            case StrictAccrualExceptionStatus.Reopened:
                switch (next)
                {
                    case StrictAccrualExceptionStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAccrualExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict accrual exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio + (double)WindowDays, 2);
    }

    public StrictAccrualException Copy()
    {
        return new StrictAccrualException(Code, Label, WindowDays, Timezone, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
