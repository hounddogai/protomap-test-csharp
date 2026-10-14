namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive leave request ledger is in its life cycle.</summary>
public enum AdaptiveLeaveRequestLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive leave request ledger, with what the planners need to act on it.</summary>
public sealed class AdaptiveLeaveRequestLedger(string code, string name, int quantity, string department, double rate, string country, string jobCode, double maximum, string ownerId, int revision, int tenureMonths, AdaptiveLeaveRequestLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public string Country { get; set; } = country;
    public string JobCode { get; set; } = jobCode;
    public double Maximum { get; set; } = maximum;
    public string OwnerId { get; set; } = ownerId;
    public int Revision { get; set; } = revision;
    public int TenureMonths { get; set; } = tenureMonths;
    public AdaptiveLeaveRequestLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive leave request ledger in status planned.</summary>
    public static AdaptiveLeaveRequestLedger NewAdaptiveLeaveRequestLedger(string code, string name)
    {
        return new AdaptiveLeaveRequestLedger(
            code,
            name,
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            0,
            0,
            AdaptiveLeaveRequestLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLeaveRequestLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLeaveRequestLedgerStatus next)
    {
        switch (Status)
        {
            case AdaptiveLeaveRequestLedgerStatus.Planned:
                switch (next)
                {
                    case AdaptiveLeaveRequestLedgerStatus.Scheduled:
                    case AdaptiveLeaveRequestLedgerStatus.Completed:
                        return true;
                }
                return false;
            case AdaptiveLeaveRequestLedgerStatus.Scheduled:
                return next == AdaptiveLeaveRequestLedgerStatus.Completed || next == AdaptiveLeaveRequestLedgerStatus.Cancelled;
            case AdaptiveLeaveRequestLedgerStatus.InProgress:
                return next == AdaptiveLeaveRequestLedgerStatus.Completed;
            case AdaptiveLeaveRequestLedgerStatus.Completed:
                return next == AdaptiveLeaveRequestLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveLeaveRequestLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive leave request ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum - (double)Quantity, 2);
    }

    public AdaptiveLeaveRequestLedger Copy()
    {
        return new AdaptiveLeaveRequestLedger(
            Code,
            Name,
            Quantity,
            Department,
            Rate,
            Country,
            JobCode,
            Maximum,
            OwnerId,
            Revision,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
