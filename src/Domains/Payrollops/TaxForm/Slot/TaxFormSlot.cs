namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form slot is in its life cycle.</summary>
public enum TaxFormSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tax form slot, with what the planners need to act on it.</summary>
public sealed class TaxFormSlot(string code, string title, double ceiling, int retries, int tenureMonths, bool active, string badge, TaxFormSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public TaxFormSlotStatus Status { get; set; } = status;

    /// <summary>Starts a tax form slot in status idle.</summary>
    public static TaxFormSlot NewTaxFormSlot(string code, string title)
    {
        return new TaxFormSlot(code, title, 0.0, 0, 0, false, "", TaxFormSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormSlotStatus next)
    {
        switch (Status)
        {
            case TaxFormSlotStatus.Idle:
                switch (next)
                {
                    case TaxFormSlotStatus.Draining:
                        return true;
                }
                return false;
            case TaxFormSlotStatus.Warming:
                switch (next)
                {
                    case TaxFormSlotStatus.Draining:
                        return true;
                }
                return false;
            case TaxFormSlotStatus.Ready:
                return next == TaxFormSlotStatus.Draining || next == TaxFormSlotStatus.Stopped;
            case TaxFormSlotStatus.Draining:
                switch (next)
                {
                    case TaxFormSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling - (double)Retries, 2);
    }

    public TaxFormSlot Copy()
    {
        return new TaxFormSlot(Code, Title, Ceiling, Retries, TenureMonths, Active, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
