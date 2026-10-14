namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default payslip budget is in its life cycle.</summary>
public enum DefaultPayslipBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default payslip budget as the payslip team keeps it.</summary>
public sealed class DefaultPayslipBudget(string code, string title, int revision, int version, int startsAt, string timezone, double threshold, DefaultPayslipBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int Version { get; set; } = version;
    public int StartsAt { get; set; } = startsAt;
    public string Timezone { get; set; } = timezone;
    public double Threshold { get; set; } = threshold;
    public DefaultPayslipBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a default payslip budget in status open.</summary>
    public static DefaultPayslipBudget NewDefaultPayslipBudget(string code, string title)
    {
        return new DefaultPayslipBudget(code, title, 0, 0, 0, "", 0.0, DefaultPayslipBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayslipBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayslipBudgetStatus next)
    {
        switch (Status)
        {
            case DefaultPayslipBudgetStatus.Open:
                switch (next)
                {
                    case DefaultPayslipBudgetStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultPayslipBudgetStatus.InReview:
                return next == DefaultPayslipBudgetStatus.Resolved;
            case DefaultPayslipBudgetStatus.Resolved:
                return next == DefaultPayslipBudgetStatus.Archived;
            case DefaultPayslipBudgetStatus.Reopened:
                switch (next)
                {
                    case DefaultPayslipBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayslipBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default payslip budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version + Threshold, 2);
    }

    public DefaultPayslipBudget Copy()
    {
        return new DefaultPayslipBudget(Code, Title, Revision, Version, StartsAt, Timezone, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
