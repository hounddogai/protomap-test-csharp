namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global payslip checkpoint is in its life cycle.</summary>
public enum GlobalPayslipCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global payslip checkpoint, with what the planners need to act on it.</summary>
public sealed class GlobalPayslipCheckpoint(string code, string name, int windowDays, double minimum, string country, double amount, string label, string region, bool pinned, GlobalPayslipCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public double Amount { get; set; } = amount;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public bool Pinned { get; set; } = pinned;
    public GlobalPayslipCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a global payslip checkpoint in status requested.</summary>
    public static GlobalPayslipCheckpoint NewGlobalPayslipCheckpoint(string code, string name)
    {
        return new GlobalPayslipCheckpoint(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            false,
            GlobalPayslipCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayslipCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayslipCheckpointStatus next)
    {
        switch (Status)
        {
            case GlobalPayslipCheckpointStatus.Requested:
                switch (next)
                {
                    case GlobalPayslipCheckpointStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalPayslipCheckpointStatus.Granted:
                return next == GlobalPayslipCheckpointStatus.Revoked;
            case GlobalPayslipCheckpointStatus.Revoked:
                switch (next)
                {
                    case GlobalPayslipCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPayslipCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global payslip checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)WindowDays - Minimum, 2);
    }

    public GlobalPayslipCheckpoint Copy()
    {
        return new GlobalPayslipCheckpoint(
            Code,
            Name,
            WindowDays,
            Minimum,
            Country,
            Amount,
            Label,
            Region,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
