namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan record is in its life cycle.</summary>
public enum ReturnPlanRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A return plan record. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanRecord(string code, string label, int attempts, int graceDays, int windowDays, ReturnPlanRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public int WindowDays { get; set; } = windowDays;
    public ReturnPlanRecordStatus Status { get; set; } = status;

    /// <summary>Starts a return plan record in status draft.</summary>
    public static ReturnPlanRecord NewReturnPlanRecord(string code, string label)
    {
        return new ReturnPlanRecord(code, label, 0, 0, 0, ReturnPlanRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanRecordStatus next)
    {
        switch (Status)
        {
            case ReturnPlanRecordStatus.Draft:
                switch (next)
                {
                    case ReturnPlanRecordStatus.Closed:
                        return true;
                }
                return false;
            case ReturnPlanRecordStatus.Pending:
                switch (next)
                {
                    case ReturnPlanRecordStatus.Active:
                    case ReturnPlanRecordStatus.Suspended:
                        return true;
                }
                return false;
            case ReturnPlanRecordStatus.Active:
                switch (next)
                {
                    case ReturnPlanRecordStatus.Suspended:
                    case ReturnPlanRecordStatus.Closed:
                        return true;
                }
                return false;
            case ReturnPlanRecordStatus.Suspended:
                switch (next)
                {
                    case ReturnPlanRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts * (double)GraceDays, 2);
    }

    public ReturnPlanRecord Copy()
    {
        return new ReturnPlanRecord(Code, Label, Attempts, GraceDays, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
