namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run window is in its life cycle.</summary>
public enum PayRunWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pay run window, with what the planners need to act on it.</summary>
public sealed class PayRunWindow(string code, string label, string reviewer, string category, double floorValue, string currency, string contactEmail, int attempts, PayRunWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Category { get; set; } = category;
    public double FloorValue { get; set; } = floorValue;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public int Attempts { get; set; } = attempts;
    public PayRunWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pay run window in status new.</summary>
    public static PayRunWindow NewPayRunWindow(string code, string label)
    {
        return new PayRunWindow(code, label, "", "", 0.0, "", "", 0, PayRunWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunWindowStatus next)
    {
        switch (Status)
        {
            case PayRunWindowStatus.New:
                switch (next)
                {
                    case PayRunWindowStatus.Running:
                        return true;
                }
                return false;
            case PayRunWindowStatus.Queued:
                switch (next)
                {
                    case PayRunWindowStatus.Done:
                        return true;
                }
                return false;
            case PayRunWindowStatus.Running:
                switch (next)
                {
                    case PayRunWindowStatus.Done:
                        return true;
                }
                return false;
            case PayRunWindowStatus.Done:
                return next == PayRunWindowStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PayRunWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(FloorValue + (double)Attempts, 2);
    }

    public PayRunWindow Copy()
    {
        return new PayRunWindow(
            Code,
            Label,
            Reviewer,
            Category,
            FloorValue,
            Currency,
            ContactEmail,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
