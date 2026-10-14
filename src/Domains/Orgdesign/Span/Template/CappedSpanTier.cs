namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped span tier is in its life cycle.</summary>
public enum CappedSpanTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A capped span tier. Its code is unique within a tenant.</summary>
public sealed class CappedSpanTier(string code, string label, int limit, double ceiling, string badge, string location, string grade, string salaryBand, CappedSpanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public string Location { get; set; } = location;
    public string Grade { get; set; } = grade;
    public string SalaryBand { get; set; } = salaryBand;
    public CappedSpanTierStatus Status { get; set; } = status;

    /// <summary>Starts a capped span tier in status idle.</summary>
    public static CappedSpanTier NewCappedSpanTier(string code, string label)
    {
        return new CappedSpanTier(code, label, 0, 0.0, "", "", "", "", CappedSpanTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSpanTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSpanTierStatus next)
    {
        switch (Status)
        {
            case CappedSpanTierStatus.Idle:
                switch (next)
                {
                    case CappedSpanTierStatus.Warming:
                        return true;
                }
                return false;
            case CappedSpanTierStatus.Warming:
                return next == CappedSpanTierStatus.Ready;
            case CappedSpanTierStatus.Ready:
                switch (next)
                {
                    case CappedSpanTierStatus.Draining:
                        return true;
                }
                return false;
            case CappedSpanTierStatus.Draining:
                return next == CappedSpanTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedSpanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped span tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped span tier"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling - (double)Limit, 2);
    }

    public CappedSpanTier Copy()
    {
        return new CappedSpanTier(Code, Label, Limit, Ceiling, Badge, Location, Grade, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
