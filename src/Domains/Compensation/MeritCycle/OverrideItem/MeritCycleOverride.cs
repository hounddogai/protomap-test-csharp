namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle override is in its life cycle.</summary>
public enum MeritCycleOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One merit cycle override, with what the planners need to act on it.</summary>
public sealed class MeritCycleOverride(string code, string label, string category, string locale, string ownerId, double amount, MeritCycleOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public MeritCycleOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle override in status new.</summary>
    public static MeritCycleOverride NewMeritCycleOverride(string code, string label)
    {
        return new MeritCycleOverride(code, label, "", "", "", 0.0, MeritCycleOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleOverrideStatus next)
    {
        switch (Status)
        {
            case MeritCycleOverrideStatus.New:
                switch (next)
                {
                    case MeritCycleOverrideStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleOverrideStatus.Queued:
                switch (next)
                {
                    case MeritCycleOverrideStatus.Running:
                    case MeritCycleOverrideStatus.Done:
                        return true;
                }
                return false;
            case MeritCycleOverrideStatus.Running:
                switch (next)
                {
                    case MeritCycleOverrideStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleOverrideStatus.Done:
                return next == MeritCycleOverrideStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MeritCycleOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public MeritCycleOverride Copy()
    {
        return new MeritCycleOverride(Code, Label, Category, Locale, OwnerId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
