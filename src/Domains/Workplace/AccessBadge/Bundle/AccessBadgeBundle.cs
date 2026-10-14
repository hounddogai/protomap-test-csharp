namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge bundle is in its life cycle.</summary>
public enum AccessBadgeBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One access badge bundle, with what the planners need to act on it.</summary>
public sealed class AccessBadgeBundle(string code, string name, string grade, double cost, string managerId, double discount, int sequence, string notes, int graceDays, string description, AccessBadgeBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public int Sequence { get; set; } = sequence;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public string Description { get; set; } = description;
    public AccessBadgeBundleStatus Status { get; set; } = status;

    /// <summary>Starts a access badge bundle in status requested.</summary>
    public static AccessBadgeBundle NewAccessBadgeBundle(string code, string name)
    {
        return new AccessBadgeBundle(code, name, "", 0.0, "", 0.0, 0, "", 0, "", AccessBadgeBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeBundleStatus next)
    {
        switch (Status)
        {
            case AccessBadgeBundleStatus.Requested:
                return next == AccessBadgeBundleStatus.Granted || next == AccessBadgeBundleStatus.Revoked;
            case AccessBadgeBundleStatus.Granted:
                switch (next)
                {
                    case AccessBadgeBundleStatus.Revoked:
                    case AccessBadgeBundleStatus.Expired:
                        return true;
                }
                return false;
            case AccessBadgeBundleStatus.Revoked:
                switch (next)
                {
                    case AccessBadgeBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Discount * (double)GraceDays, 2);
    }

    public AccessBadgeBundle Copy()
    {
        return new AccessBadgeBundle(
            Code,
            Name,
            Grade,
            Cost,
            ManagerId,
            Discount,
            Sequence,
            Notes,
            GraceDays,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
