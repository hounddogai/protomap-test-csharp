namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped career path budget is in its life cycle.</summary>
public enum CappedCareerPathBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped career path budget. Its code is unique within a tenant.</summary>
public sealed class CappedCareerPathBudget(string code, string label, string ownerId, string grade, int sequence, int tenureMonths, int endsAt, string name, int quantity, double discount, CappedCareerPathBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Grade { get; set; } = grade;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public int EndsAt { get; set; } = endsAt;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Discount { get; set; } = discount;
    public CappedCareerPathBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a capped career path budget in status planned.</summary>
    public static CappedCareerPathBudget NewCappedCareerPathBudget(string code, string label)
    {
        return new CappedCareerPathBudget(
            code,
            label,
            "",
            "",
            0,
            0,
            0,
            "",
            0,
            0.0,
            CappedCareerPathBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCareerPathBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCareerPathBudgetStatus next)
    {
        switch (Status)
        {
            case CappedCareerPathBudgetStatus.Planned:
                switch (next)
                {
                    case CappedCareerPathBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedCareerPathBudgetStatus.Scheduled:
                switch (next)
                {
                    case CappedCareerPathBudgetStatus.InProgress:
                    case CappedCareerPathBudgetStatus.Completed:
                        return true;
                }
                return false;
            case CappedCareerPathBudgetStatus.InProgress:
                switch (next)
                {
                    case CappedCareerPathBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedCareerPathBudgetStatus.Completed:
                switch (next)
                {
                    case CappedCareerPathBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCareerPathBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped career path budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt + (double)Quantity, 2);
    }

    public CappedCareerPathBudget Copy()
    {
        return new CappedCareerPathBudget(
            Code,
            Label,
            OwnerId,
            Grade,
            Sequence,
            TenureMonths,
            EndsAt,
            Name,
            Quantity,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
