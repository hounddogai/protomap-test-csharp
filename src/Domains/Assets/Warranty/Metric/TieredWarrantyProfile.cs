namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered warranty profile is in its life cycle.</summary>
public enum TieredWarrantyProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tiered warranty profile, with what the planners need to act on it.</summary>
public sealed class TieredWarrantyProfile(string code, string name, double fte, double score, double rate, string segment, TieredWarrantyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public string Segment { get; set; } = segment;
    public TieredWarrantyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a tiered warranty profile in status new.</summary>
    public static TieredWarrantyProfile NewTieredWarrantyProfile(string code, string name)
    {
        return new TieredWarrantyProfile(code, name, 0.0, 0.0, 0.0, "", TieredWarrantyProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredWarrantyProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredWarrantyProfileStatus next)
    {
        switch (Status)
        {
            case TieredWarrantyProfileStatus.New:
                switch (next)
                {
                    case TieredWarrantyProfileStatus.Queued:
                    case TieredWarrantyProfileStatus.Running:
                        return true;
                }
                return false;
            case TieredWarrantyProfileStatus.Queued:
                return next == TieredWarrantyProfileStatus.Running || next == TieredWarrantyProfileStatus.Failed;
            case TieredWarrantyProfileStatus.Running:
                return next == TieredWarrantyProfileStatus.Done;
            case TieredWarrantyProfileStatus.Done:
                return next == TieredWarrantyProfileStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TieredWarrantyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered warranty profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Score + Rate, 2);
    }

    public TieredWarrantyProfile Copy()
    {
        return new TieredWarrantyProfile(Code, Name, Fte, Score, Rate, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
