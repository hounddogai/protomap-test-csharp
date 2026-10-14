namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped adjustment agreement is in its life cycle.</summary>
public enum CappedAdjustmentAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped adjustment agreement as the adjustment team keeps it.</summary>
public sealed class CappedAdjustmentAgreement(string code, string name, int retries, double cost, List<string> tags, int quantity, string location, double budget, CappedAdjustmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public string Location { get; set; } = location;
    public double Budget { get; set; } = budget;
    public CappedAdjustmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a capped adjustment agreement in status planned.</summary>
    public static CappedAdjustmentAgreement NewCappedAdjustmentAgreement(string code, string name)
    {
        return new CappedAdjustmentAgreement(
            code,
            name,
            0,
            0.0,
            new List<string>(),
            0,
            "",
            0.0,
            CappedAdjustmentAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAdjustmentAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAdjustmentAgreementStatus next)
    {
        switch (Status)
        {
            case CappedAdjustmentAgreementStatus.Planned:
                switch (next)
                {
                    case CappedAdjustmentAgreementStatus.Scheduled:
                    case CappedAdjustmentAgreementStatus.Completed:
                        return true;
                }
                return false;
            case CappedAdjustmentAgreementStatus.Scheduled:
                return next == CappedAdjustmentAgreementStatus.InProgress || next == CappedAdjustmentAgreementStatus.Cancelled;
            case CappedAdjustmentAgreementStatus.InProgress:
                return next == CappedAdjustmentAgreementStatus.Completed;
            case CappedAdjustmentAgreementStatus.Completed:
                switch (next)
                {
                    case CappedAdjustmentAgreementStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAdjustmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped adjustment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget + Cost, 2);
    }

    public CappedAdjustmentAgreement Copy()
    {
        return new CappedAdjustmentAgreement(Code, Name, Retries, Cost, Tags, Quantity, Location, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
