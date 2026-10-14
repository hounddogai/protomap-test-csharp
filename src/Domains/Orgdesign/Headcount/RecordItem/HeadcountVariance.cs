namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount variance is in its life cycle.</summary>
public enum HeadcountVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A headcount variance as the headcount team keeps it.</summary>
public sealed class HeadcountVariance(string code, string label, string unit, int expiresAt, double score, HeadcountVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Score { get; set; } = score;
    public HeadcountVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a headcount variance in status new.</summary>
    public static HeadcountVariance NewHeadcountVariance(string code, string label)
    {
        return new HeadcountVariance(code, label, "", 0, 0.0, HeadcountVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountVarianceStatus next)
    {
        switch (Status)
        {
            case HeadcountVarianceStatus.New:
                return next == HeadcountVarianceStatus.Running || next == HeadcountVarianceStatus.Failed;
            case HeadcountVarianceStatus.Queued:
                switch (next)
                {
                    case HeadcountVarianceStatus.Done:
                    case HeadcountVarianceStatus.Failed:
                        return true;
                }
                return false;
            case HeadcountVarianceStatus.Running:
                return next == HeadcountVarianceStatus.Failed;
            case HeadcountVarianceStatus.Done:
                return next == HeadcountVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(HeadcountVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt * Score, 2);
    }

    public HeadcountVariance Copy()
    {
        return new HeadcountVariance(Code, Label, Unit, ExpiresAt, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
