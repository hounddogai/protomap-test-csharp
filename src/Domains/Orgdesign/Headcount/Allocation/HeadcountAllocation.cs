namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount allocation is in its life cycle.</summary>
public enum HeadcountAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a headcount allocation and its current status.</summary>
public sealed class HeadcountAllocation(string code, string label, int attempts, string category, string notes, string skill, string unit, string currency, string channel, double rating, HeadcountAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public double Rating { get; set; } = rating;
    public HeadcountAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a headcount allocation in status requested.</summary>
    public static HeadcountAllocation NewHeadcountAllocation(string code, string label)
    {
        return new HeadcountAllocation(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            "",
            "",
            0.0,
            HeadcountAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountAllocationStatus next)
    {
        switch (Status)
        {
            case HeadcountAllocationStatus.Requested:
                switch (next)
                {
                    case HeadcountAllocationStatus.Revoked:
                    case HeadcountAllocationStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountAllocationStatus.Granted:
                switch (next)
                {
                    case HeadcountAllocationStatus.Revoked:
                        return true;
                }
                return false;
            case HeadcountAllocationStatus.Revoked:
                switch (next)
                {
                    case HeadcountAllocationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Attempts - Rating, 2);
    }

    public HeadcountAllocation Copy()
    {
        return new HeadcountAllocation(
            Code,
            Label,
            Attempts,
            Category,
            Notes,
            Skill,
            Unit,
            Currency,
            Channel,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
