namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model agreement is in its life cycle.</summary>
public enum CostModelAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A cost model agreement. Its code is unique within a tenant.</summary>
public sealed class CostModelAgreement(string code, string name, int attempts, double budget, double amount, CostModelAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double Budget { get; set; } = budget;
    public double Amount { get; set; } = amount;
    public CostModelAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a cost model agreement in status planned.</summary>
    public static CostModelAgreement NewCostModelAgreement(string code, string name)
    {
        return new CostModelAgreement(code, name, 0, 0.0, 0.0, CostModelAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelAgreementStatus next)
    {
        switch (Status)
        {
            case CostModelAgreementStatus.Planned:
                switch (next)
                {
                    case CostModelAgreementStatus.InProgress:
                        return true;
                }
                return false;
            case CostModelAgreementStatus.Scheduled:
                return next == CostModelAgreementStatus.InProgress || next == CostModelAgreementStatus.Completed;
            case CostModelAgreementStatus.InProgress:
                switch (next)
                {
                    case CostModelAgreementStatus.Completed:
                    case CostModelAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case CostModelAgreementStatus.Completed:
                return next == CostModelAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CostModelAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round(Budget + (double)Attempts, 2);
    }

    public CostModelAgreement Copy()
    {
        return new CostModelAgreement(Code, Name, Attempts, Budget, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
