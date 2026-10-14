namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat budget is in its life cycle.</summary>
public enum LicenseSeatBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a license seat budget and its current status.</summary>
public sealed class LicenseSeatBudget(string code, string label, double ratio, double cost, string jobCode, double amount, double hours, double minimum, string locale, LicenseSeatBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public LicenseSeatBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a license seat budget in status new.</summary>
    public static LicenseSeatBudget NewLicenseSeatBudget(string code, string label)
    {
        return new LicenseSeatBudget(code, label, 0.0, 0.0, "", 0.0, 0.0, 0.0, "", LicenseSeatBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatBudgetStatus next)
    {
        switch (Status)
        {
            case LicenseSeatBudgetStatus.New:
                switch (next)
                {
                    case LicenseSeatBudgetStatus.Done:
                    case LicenseSeatBudgetStatus.Failed:
                        return true;
                }
                return false;
            case LicenseSeatBudgetStatus.Queued:
                return next == LicenseSeatBudgetStatus.Done || next == LicenseSeatBudgetStatus.Failed;
            case LicenseSeatBudgetStatus.Running:
                switch (next)
                {
                    case LicenseSeatBudgetStatus.Done:
                        return true;
                }
                return false;
            case LicenseSeatBudgetStatus.Done:
                return next == LicenseSeatBudgetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LicenseSeatBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round(Minimum + Ratio, 2);
    }

    public LicenseSeatBudget Copy()
    {
        return new LicenseSeatBudget(Code, Label, Ratio, Cost, JobCode, Amount, Hours, Minimum, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
