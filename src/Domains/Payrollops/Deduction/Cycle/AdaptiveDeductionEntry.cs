namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive deduction entry is in its life cycle.</summary>
public enum AdaptiveDeductionEntryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive deduction entry and its current status.</summary>
public sealed class AdaptiveDeductionEntry(string code, string title, string category, string source, int sequence, double budget, string managerId, bool active, AdaptiveDeductionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Source { get; set; } = source;
    public int Sequence { get; set; } = sequence;
    public double Budget { get; set; } = budget;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public AdaptiveDeductionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive deduction entry in status idle.</summary>
    public static AdaptiveDeductionEntry NewAdaptiveDeductionEntry(string code, string title)
    {
        return new AdaptiveDeductionEntry(code, title, "", "", 0, 0.0, "", false, AdaptiveDeductionEntryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveDeductionEntryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveDeductionEntryStatus next)
    {
        switch (Status)
        {
            case AdaptiveDeductionEntryStatus.Idle:
                switch (next)
                {
                    case AdaptiveDeductionEntryStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveDeductionEntryStatus.Warming:
                return next == AdaptiveDeductionEntryStatus.Ready || next == AdaptiveDeductionEntryStatus.Draining;
            case AdaptiveDeductionEntryStatus.Ready:
                return next == AdaptiveDeductionEntryStatus.Stopped;
            case AdaptiveDeductionEntryStatus.Draining:
                switch (next)
                {
                    case AdaptiveDeductionEntryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveDeductionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive deduction entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence + Budget, 2);
    }

    public AdaptiveDeductionEntry Copy()
    {
        return new AdaptiveDeductionEntry(
            Code,
            Title,
            Category,
            Source,
            Sequence,
            Budget,
            ManagerId,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
