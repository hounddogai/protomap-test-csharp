namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat budget is in its life cycle.</summary>
public enum FallbackLicenseSeatBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback license seat budget, with what the planners need to act on it.</summary>
public sealed class FallbackLicenseSeatBudget(string code, string title, string channel, string salaryBand, string notes, string label, int quantity, FallbackLicenseSeatBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string SalaryBand { get; set; } = salaryBand;
    public string Notes { get; set; } = notes;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public FallbackLicenseSeatBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat budget in status requested.</summary>
    public static FallbackLicenseSeatBudget NewFallbackLicenseSeatBudget(string code, string title)
    {
        return new FallbackLicenseSeatBudget(
            code,
            title,
            "",
            "",
            "",
            "",
            0,
            FallbackLicenseSeatBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatBudgetStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatBudgetStatus.Requested:
                switch (next)
                {
                    case FallbackLicenseSeatBudgetStatus.Granted:
                    case FallbackLicenseSeatBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackLicenseSeatBudgetStatus.Granted:
                switch (next)
                {
                    case FallbackLicenseSeatBudgetStatus.Revoked:
                    case FallbackLicenseSeatBudgetStatus.Expired:
                        return true;
                }
                return false;
            case FallbackLicenseSeatBudgetStatus.Revoked:
                return next == FallbackLicenseSeatBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback license seat budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public FallbackLicenseSeatBudget Copy()
    {
        return new FallbackLicenseSeatBudget(Code, Title, Channel, SalaryBand, Notes, Label, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
