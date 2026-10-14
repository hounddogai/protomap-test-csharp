namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg window is in its life cycle.</summary>
public enum ReorgWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One reorg window, with what the planners need to act on it.</summary>
public sealed class ReorgWindow(string code, string name, string country, string unit, double amount, ReorgWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public ReorgWindowStatus Status { get; set; } = status;

    /// <summary>Starts a reorg window in status new.</summary>
    public static ReorgWindow NewReorgWindow(string code, string name)
    {
        return new ReorgWindow(code, name, "", "", 0.0, ReorgWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgWindowStatus next)
    {
        switch (Status)
        {
            case ReorgWindowStatus.New:
                switch (next)
                {
                    case ReorgWindowStatus.Running:
                    case ReorgWindowStatus.Failed:
                        return true;
                }
                return false;
            case ReorgWindowStatus.Queued:
                switch (next)
                {
                    case ReorgWindowStatus.Done:
                        return true;
                }
                return false;
            case ReorgWindowStatus.Running:
                switch (next)
                {
                    case ReorgWindowStatus.Failed:
                        return true;
                }
                return false;
            case ReorgWindowStatus.Done:
                switch (next)
                {
                    case ReorgWindowStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ReorgWindow Copy()
    {
        return new ReorgWindow(Code, Name, Country, Unit, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
