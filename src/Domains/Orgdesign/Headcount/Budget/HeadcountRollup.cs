namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount rollup is in its life cycle.</summary>
public enum HeadcountRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One headcount rollup, with what the planners need to act on it.</summary>
public sealed class HeadcountRollup(string code, string label, bool pinned, string name, int tenureMonths, string title, double discount, HeadcountRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public HeadcountRollupStatus Status { get; set; } = status;

    /// <summary>Starts a headcount rollup in status idle.</summary>
    public static HeadcountRollup NewHeadcountRollup(string code, string label)
    {
        return new HeadcountRollup(code, label, false, "", 0, "", 0.0, HeadcountRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountRollupStatus next)
    {
        switch (Status)
        {
            case HeadcountRollupStatus.Idle:
                switch (next)
                {
                    case HeadcountRollupStatus.Ready:
                    case HeadcountRollupStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountRollupStatus.Warming:
                return next == HeadcountRollupStatus.Draining;
            case HeadcountRollupStatus.Ready:
                return next == HeadcountRollupStatus.Draining || next == HeadcountRollupStatus.Stopped;
            case HeadcountRollupStatus.Draining:
                switch (next)
                {
                    case HeadcountRollupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"headcount rollup"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)TenureMonths * Discount, 2);
    }

    public HeadcountRollup Copy()
    {
        return new HeadcountRollup(Code, Label, Pinned, Name, TenureMonths, Title, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
