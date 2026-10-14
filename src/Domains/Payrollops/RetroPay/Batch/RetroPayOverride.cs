namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay override is in its life cycle.</summary>
public enum RetroPayOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a retro pay override and its current status.</summary>
public sealed class RetroPayOverride(string code, string label, string reviewer, int limit, int revision, int graceDays, double threshold, string unit, string currency, RetroPayOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int Limit { get; set; } = limit;
    public int Revision { get; set; } = revision;
    public int GraceDays { get; set; } = graceDays;
    public double Threshold { get; set; } = threshold;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public RetroPayOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay override in status created.</summary>
    public static RetroPayOverride NewRetroPayOverride(string code, string label)
    {
        return new RetroPayOverride(code, label, "", 0, 0, 0, 0.0, "", "", RetroPayOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayOverrideStatus next)
    {
        switch (Status)
        {
            case RetroPayOverrideStatus.Created:
                return next == RetroPayOverrideStatus.Retired;
            case RetroPayOverrideStatus.Validated:
                return next == RetroPayOverrideStatus.Published || next == RetroPayOverrideStatus.Retired;
            case RetroPayOverrideStatus.Published:
                return next == RetroPayOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit - (double)GraceDays, 2);
    }

    public RetroPayOverride Copy()
    {
        return new RetroPayOverride(
            Code,
            Label,
            Reviewer,
            Limit,
            Revision,
            GraceDays,
            Threshold,
            Unit,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
