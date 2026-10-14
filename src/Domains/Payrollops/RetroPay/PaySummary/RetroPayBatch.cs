namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay batch is in its life cycle.</summary>
public enum RetroPayBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A retro pay batch. Its code is unique within a tenant.</summary>
public sealed class RetroPayBatch(string code, string name, int version, string reviewer, double score, double threshold, RetroPayBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public double Threshold { get; set; } = threshold;
    public RetroPayBatchStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay batch in status created.</summary>
    public static RetroPayBatch NewRetroPayBatch(string code, string name)
    {
        return new RetroPayBatch(code, name, 0, "", 0.0, 0.0, RetroPayBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayBatchStatus next)
    {
        switch (Status)
        {
            case RetroPayBatchStatus.Created:
                return next == RetroPayBatchStatus.Validated;
            case RetroPayBatchStatus.Validated:
                return next == RetroPayBatchStatus.Published || next == RetroPayBatchStatus.Retired;
            case RetroPayBatchStatus.Published:
                switch (next)
                {
                    case RetroPayBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"retro pay batch"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold - Score, 2);
    }

    public RetroPayBatch Copy()
    {
        return new RetroPayBatch(Code, Name, Version, Reviewer, Score, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
