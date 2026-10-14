namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global adjustment bundle is in its life cycle.</summary>
public enum GlobalAdjustmentBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global adjustment bundle, with what the planners need to act on it.</summary>
public sealed class GlobalAdjustmentBundle(string code, string name, double rate, double rating, double budget, GlobalAdjustmentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Rating { get; set; } = rating;
    public double Budget { get; set; } = budget;
    public GlobalAdjustmentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a global adjustment bundle in status new.</summary>
    public static GlobalAdjustmentBundle NewGlobalAdjustmentBundle(string code, string name)
    {
        return new GlobalAdjustmentBundle(code, name, 0.0, 0.0, 0.0, GlobalAdjustmentBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAdjustmentBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAdjustmentBundleStatus next)
    {
        switch (Status)
        {
            case GlobalAdjustmentBundleStatus.New:
                switch (next)
                {
                    case GlobalAdjustmentBundleStatus.Queued:
                    case GlobalAdjustmentBundleStatus.Failed:
                        return true;
                }
                return false;
            case GlobalAdjustmentBundleStatus.Queued:
                switch (next)
                {
                    case GlobalAdjustmentBundleStatus.Running:
                        return true;
                }
                return false;
            case GlobalAdjustmentBundleStatus.Running:
                switch (next)
                {
                    case GlobalAdjustmentBundleStatus.Done:
                    case GlobalAdjustmentBundleStatus.Failed:
                        return true;
                }
                return false;
            case GlobalAdjustmentBundleStatus.Done:
                switch (next)
                {
                    case GlobalAdjustmentBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAdjustmentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global adjustment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global adjustment bundle"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Budget + Rating, 2);
    }

    public GlobalAdjustmentBundle Copy()
    {
        return new GlobalAdjustmentBundle(Code, Name, Rate, Rating, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
