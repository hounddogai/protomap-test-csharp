namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay agreement is in its life cycle.</summary>
public enum RetroPayAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a retro pay agreement and its current status.</summary>
public sealed class RetroPayAgreement(string code, string name, double ceiling, string employeeId, double floorValue, string contactEmail, string managerId, int version, RetroPayAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string EmployeeId { get; set; } = employeeId;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public int Version { get; set; } = version;
    public RetroPayAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay agreement in status planned.</summary>
    public static RetroPayAgreement NewRetroPayAgreement(string code, string name)
    {
        return new RetroPayAgreement(code, name, 0.0, "", 0.0, "", "", 0, RetroPayAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayAgreementStatus next)
    {
        switch (Status)
        {
            case RetroPayAgreementStatus.Planned:
                switch (next)
                {
                    case RetroPayAgreementStatus.Scheduled:
                    case RetroPayAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayAgreementStatus.Scheduled:
                switch (next)
                {
                    case RetroPayAgreementStatus.Completed:
                        return true;
                }
                return false;
            case RetroPayAgreementStatus.InProgress:
                switch (next)
                {
                    case RetroPayAgreementStatus.Completed:
                        return true;
                }
                return false;
            case RetroPayAgreementStatus.Completed:
                return next == RetroPayAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RetroPayAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Version - Ceiling, 2);
    }

    public RetroPayAgreement Copy()
    {
        return new RetroPayAgreement(
            Code,
            Name,
            Ceiling,
            EmployeeId,
            FloorValue,
            ContactEmail,
            ManagerId,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
