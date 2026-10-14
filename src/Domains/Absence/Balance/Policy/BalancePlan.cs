namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance plan is in its life cycle.</summary>
public enum BalancePlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a balance plan and its current status.</summary>
public sealed class BalancePlan(string code, string title, string contactEmail, double ceiling, int durationMinutes, BalancePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ceiling { get; set; } = ceiling;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BalancePlanStatus Status { get; set; } = status;

    /// <summary>Starts a balance plan in status draft.</summary>
    public static BalancePlan NewBalancePlan(string code, string title)
    {
        return new BalancePlan(code, title, "", 0.0, 0, BalancePlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalancePlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalancePlanStatus next)
    {
        switch (Status)
        {
            case BalancePlanStatus.Draft:
                return next == BalancePlanStatus.Pending || next == BalancePlanStatus.Closed;
            case BalancePlanStatus.Pending:
                return next == BalancePlanStatus.Closed;
            case BalancePlanStatus.Active:
                return next == BalancePlanStatus.Suspended || next == BalancePlanStatus.Closed;
            case BalancePlanStatus.Suspended:
                return next == BalancePlanStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BalancePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ceiling - (double)DurationMinutes, 2);
    }

    public BalancePlan Copy()
    {
        return new BalancePlan(Code, Title, ContactEmail, Ceiling, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
