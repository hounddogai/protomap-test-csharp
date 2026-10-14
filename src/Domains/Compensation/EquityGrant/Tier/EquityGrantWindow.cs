namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant window is in its life cycle.</summary>
public enum EquityGrantWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A equity grant window as the equity grant team keeps it.</summary>
public sealed class EquityGrantWindow(string code, string label, bool visible, double minimum, int limit, double score, EquityGrantWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public EquityGrantWindowStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant window in status idle.</summary>
    public static EquityGrantWindow NewEquityGrantWindow(string code, string label)
    {
        return new EquityGrantWindow(code, label, false, 0.0, 0, 0.0, EquityGrantWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantWindowStatus next)
    {
        switch (Status)
        {
            case EquityGrantWindowStatus.Idle:
                switch (next)
                {
                    case EquityGrantWindowStatus.Warming:
                    case EquityGrantWindowStatus.Draining:
                        return true;
                }
                return false;
            case EquityGrantWindowStatus.Warming:
                switch (next)
                {
                    case EquityGrantWindowStatus.Draining:
                        return true;
                }
                return false;
            case EquityGrantWindowStatus.Ready:
                switch (next)
                {
                    case EquityGrantWindowStatus.Draining:
                    case EquityGrantWindowStatus.Stopped:
                        return true;
                }
                return false;
            case EquityGrantWindowStatus.Draining:
                switch (next)
                {
                    case EquityGrantWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetExposure()
    {
        return Math.Round(Score * Minimum, 2);
    }

    public EquityGrantWindow Copy()
    {
        return new EquityGrantWindow(Code, Label, Visible, Minimum, Limit, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
