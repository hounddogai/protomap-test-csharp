namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling reorg bundle is in its life cycle.</summary>
public enum RollingReorgBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling reorg bundle, with what the planners need to act on it.</summary>
public sealed class RollingReorgBundle(string code, string label, string jobCode, int tenureMonths, string grade, double rating, double rate, string name, string ownerId, string currency, RollingReorgBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string Currency { get; set; } = currency;
    public RollingReorgBundleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling reorg bundle in status new.</summary>
    public static RollingReorgBundle NewRollingReorgBundle(string code, string label)
    {
        return new RollingReorgBundle(code, label, "", 0, "", 0.0, 0.0, "", "", "", RollingReorgBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingReorgBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingReorgBundleStatus next)
    {
        switch (Status)
        {
            case RollingReorgBundleStatus.New:
                switch (next)
                {
                    case RollingReorgBundleStatus.Queued:
                        return true;
                }
                return false;
            case RollingReorgBundleStatus.Queued:
                switch (next)
                {
                    case RollingReorgBundleStatus.Running:
                    case RollingReorgBundleStatus.Failed:
                        return true;
                }
                return false;
            case RollingReorgBundleStatus.Running:
                return next == RollingReorgBundleStatus.Done || next == RollingReorgBundleStatus.Failed;
            case RollingReorgBundleStatus.Done:
                switch (next)
                {
                    case RollingReorgBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingReorgBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling reorg bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rate * Rating, 2);
    }

    public RollingReorgBundle Copy()
    {
        return new RollingReorgBundle(
            Code,
            Label,
            JobCode,
            TenureMonths,
            Grade,
            Rating,
            Rate,
            Name,
            OwnerId,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
