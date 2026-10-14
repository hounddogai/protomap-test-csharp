namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount baseline is in its life cycle.</summary>
public enum HeadcountBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One headcount baseline, with what the planners need to act on it.</summary>
public sealed class HeadcountBaseline(string code, string name, double amount, string reviewer, int quantity, double maximum, HeadcountBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Reviewer { get; set; } = reviewer;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public HeadcountBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a headcount baseline in status proposed.</summary>
    public static HeadcountBaseline NewHeadcountBaseline(string code, string name)
    {
        return new HeadcountBaseline(code, name, 0.0, "", 0, 0.0, HeadcountBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountBaselineStatus next)
    {
        switch (Status)
        {
            case HeadcountBaselineStatus.Proposed:
                switch (next)
                {
                    case HeadcountBaselineStatus.Approved:
                    case HeadcountBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountBaselineStatus.Approved:
                switch (next)
                {
                    case HeadcountBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountBaselineStatus.Rejected:
                switch (next)
                {
                    case HeadcountBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Amount * Maximum, 2);
    }

    public HeadcountBaseline Copy()
    {
        return new HeadcountBaseline(Code, Name, Amount, Reviewer, Quantity, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
