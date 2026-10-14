namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan item is in its life cycle.</summary>
public enum SuccessionPlanItemStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A succession plan item as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanItem(string code, string title, string source, int batchSize, int priority, SuccessionPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public int Priority { get; set; } = priority;
    public SuccessionPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan item in status idle.</summary>
    public static SuccessionPlanItem NewSuccessionPlanItem(string code, string title)
    {
        return new SuccessionPlanItem(code, title, "", 0, 0, SuccessionPlanItemStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanItemStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanItemStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanItemStatus.Idle:
                switch (next)
                {
                    case SuccessionPlanItemStatus.Warming:
                        return true;
                }
                return false;
            case SuccessionPlanItemStatus.Warming:
                switch (next)
                {
                    case SuccessionPlanItemStatus.Ready:
                        return true;
                }
                return false;
            case SuccessionPlanItemStatus.Ready:
                switch (next)
                {
                    case SuccessionPlanItemStatus.Draining:
                    case SuccessionPlanItemStatus.Stopped:
                        return true;
                }
                return false;
            case SuccessionPlanItemStatus.Draining:
                return next == SuccessionPlanItemStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"succession plan item"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Priority * (double)BatchSize, 2);
    }

    public SuccessionPlanItem Copy()
    {
        return new SuccessionPlanItem(Code, Title, Source, BatchSize, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
