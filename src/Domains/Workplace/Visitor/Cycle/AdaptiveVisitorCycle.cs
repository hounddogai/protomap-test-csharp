namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive visitor cycle is in its life cycle.</summary>
public enum AdaptiveVisitorCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive visitor cycle as the visitor team keeps it.</summary>
public sealed class AdaptiveVisitorCycle(string code, string title, string employeeId, string contactEmail, int retries, double score, int leaveDays, string channel, double amount, AdaptiveVisitorCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public int Retries { get; set; } = retries;
    public double Score { get; set; } = score;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public AdaptiveVisitorCycleStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive visitor cycle in status created.</summary>
    public static AdaptiveVisitorCycle NewAdaptiveVisitorCycle(string code, string title)
    {
        return new AdaptiveVisitorCycle(code, title, "", "", 0, 0.0, 0, "", 0.0, AdaptiveVisitorCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveVisitorCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveVisitorCycleStatus next)
    {
        switch (Status)
        {
            case AdaptiveVisitorCycleStatus.Created:
                switch (next)
                {
                    case AdaptiveVisitorCycleStatus.Published:
                    case AdaptiveVisitorCycleStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveVisitorCycleStatus.Validated:
                switch (next)
                {
                    case AdaptiveVisitorCycleStatus.Published:
                        return true;
                }
                return false;
            case AdaptiveVisitorCycleStatus.Published:
                return next == AdaptiveVisitorCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveVisitorCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive visitor cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount * (double)Retries, 2);
    }

    public AdaptiveVisitorCycle Copy()
    {
        return new AdaptiveVisitorCycle(
            Code,
            Title,
            EmployeeId,
            ContactEmail,
            Retries,
            Score,
            LeaveDays,
            Channel,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
