namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback refresh policy is in its life cycle.</summary>
public enum FallbackRefreshPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a fallback refresh policy and its current status.</summary>
public sealed class FallbackRefreshPolicy(string code, string label, string source, string costCenter, int limit, bool archived, FallbackRefreshPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string CostCenter { get; set; } = costCenter;
    public int Limit { get; set; } = limit;
    public bool Archived { get; set; } = archived;
    public FallbackRefreshPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a fallback refresh policy in status new.</summary>
    public static FallbackRefreshPolicy NewFallbackRefreshPolicy(string code, string label)
    {
        return new FallbackRefreshPolicy(code, label, "", "", 0, false, FallbackRefreshPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRefreshPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRefreshPolicyStatus next)
    {
        switch (Status)
        {
            case FallbackRefreshPolicyStatus.New:
                switch (next)
                {
                    case FallbackRefreshPolicyStatus.Queued:
                        return true;
                }
                return false;
            case FallbackRefreshPolicyStatus.Queued:
                return next == FallbackRefreshPolicyStatus.Running || next == FallbackRefreshPolicyStatus.Done;
            case FallbackRefreshPolicyStatus.Running:
                return next == FallbackRefreshPolicyStatus.Done || next == FallbackRefreshPolicyStatus.Failed;
            case FallbackRefreshPolicyStatus.Done:
                switch (next)
                {
                    case FallbackRefreshPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackRefreshPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback refresh policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public FallbackRefreshPolicy Copy()
    {
        return new FallbackRefreshPolicy(Code, Label, Source, CostCenter, Limit, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
