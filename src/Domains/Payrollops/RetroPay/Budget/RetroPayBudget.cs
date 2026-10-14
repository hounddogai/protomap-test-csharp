namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay budget is in its life cycle.</summary>
public enum RetroPayBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A retro pay budget. Its code is unique within a tenant.</summary>
public sealed class RetroPayBudget(string code, string label, double rate, string team, int batchSize, string externalRef, string source, double ceiling, string phone, bool visible, RetroPayBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public string Source { get; set; } = source;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public RetroPayBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay budget in status planned.</summary>
    public static RetroPayBudget NewRetroPayBudget(string code, string label)
    {
        return new RetroPayBudget(code, label, 0.0, "", 0, "", "", 0.0, "", false, RetroPayBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayBudgetStatus next)
    {
        switch (Status)
        {
            case RetroPayBudgetStatus.Planned:
                switch (next)
                {
                    case RetroPayBudgetStatus.InProgress:
                    case RetroPayBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayBudgetStatus.Scheduled:
                switch (next)
                {
                    case RetroPayBudgetStatus.Completed:
                    case RetroPayBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayBudgetStatus.InProgress:
                return next == RetroPayBudgetStatus.Completed || next == RetroPayBudgetStatus.Cancelled;
            case RetroPayBudgetStatus.Completed:
                return next == RetroPayBudgetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RetroPayBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate * (double)BatchSize, 2);
    }

    public RetroPayBudget Copy()
    {
        return new RetroPayBudget(
            Code,
            Label,
            Rate,
            Team,
            BatchSize,
            ExternalRef,
            Source,
            Ceiling,
            Phone,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
