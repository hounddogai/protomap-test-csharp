namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy retro pay cycle is in its life cycle.</summary>
public enum LegacyRetroPayCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy retro pay cycle and its current status.</summary>
public sealed class LegacyRetroPayCycle(string code, string title, string notes, string locale, bool visible, string label, double amount, LegacyRetroPayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public LegacyRetroPayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy retro pay cycle in status new.</summary>
    public static LegacyRetroPayCycle NewLegacyRetroPayCycle(string code, string title)
    {
        return new LegacyRetroPayCycle(code, title, "", "", false, "", 0.0, LegacyRetroPayCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRetroPayCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRetroPayCycleStatus next)
    {
        switch (Status)
        {
            case LegacyRetroPayCycleStatus.New:
                return next == LegacyRetroPayCycleStatus.Queued;
            case LegacyRetroPayCycleStatus.Queued:
                switch (next)
                {
                    case LegacyRetroPayCycleStatus.Done:
                        return true;
                }
                return false;
            case LegacyRetroPayCycleStatus.Running:
                switch (next)
                {
                    case LegacyRetroPayCycleStatus.Failed:
                        return true;
                }
                return false;
            case LegacyRetroPayCycleStatus.Done:
                switch (next)
                {
                    case LegacyRetroPayCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyRetroPayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy retro pay cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy retro pay cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public LegacyRetroPayCycle Copy()
    {
        return new LegacyRetroPayCycle(Code, Title, Notes, Locale, Visible, Label, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
