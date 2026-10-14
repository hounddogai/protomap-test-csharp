namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered scenario draft is in its life cycle.</summary>
public enum TieredScenarioDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a tiered scenario draft and its current status.</summary>
public sealed class TieredScenarioDraft(string code, string label, int sequence, int retries, double threshold, int limit, TieredScenarioDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public int Limit { get; set; } = limit;
    public TieredScenarioDraftStatus Status { get; set; } = status;

    /// <summary>Starts a tiered scenario draft in status planned.</summary>
    public static TieredScenarioDraft NewTieredScenarioDraft(string code, string label)
    {
        return new TieredScenarioDraft(code, label, 0, 0, 0.0, 0, TieredScenarioDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredScenarioDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredScenarioDraftStatus next)
    {
        switch (Status)
        {
            case TieredScenarioDraftStatus.Planned:
                return next == TieredScenarioDraftStatus.Completed || next == TieredScenarioDraftStatus.Cancelled;
            case TieredScenarioDraftStatus.Scheduled:
                return next == TieredScenarioDraftStatus.InProgress;
            case TieredScenarioDraftStatus.InProgress:
                switch (next)
                {
                    case TieredScenarioDraftStatus.Completed:
                        return true;
                }
                return false;
            case TieredScenarioDraftStatus.Completed:
                return next == TieredScenarioDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredScenarioDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered scenario draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tiered scenario draft"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence * (double)Retries, 2);
    }

    public TieredScenarioDraft Copy()
    {
        return new TieredScenarioDraft(Code, Label, Sequence, Retries, Threshold, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
