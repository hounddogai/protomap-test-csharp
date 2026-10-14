namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge plan is in its life cycle.</summary>
public enum AccessBadgePlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a access badge plan and its current status.</summary>
public sealed class AccessBadgePlan(string code, string name, int batchSize, bool approved, string locale, double fte, int limit, string phone, string currency, AccessBadgePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public bool Approved { get; set; } = approved;
    public string Locale { get; set; } = locale;
    public double Fte { get; set; } = fte;
    public int Limit { get; set; } = limit;
    public string Phone { get; set; } = phone;
    public string Currency { get; set; } = currency;
    public AccessBadgePlanStatus Status { get; set; } = status;

    /// <summary>Starts a access badge plan in status draft.</summary>
    public static AccessBadgePlan NewAccessBadgePlan(string code, string name)
    {
        return new AccessBadgePlan(code, name, 0, false, "", 0.0, 0, "", "", AccessBadgePlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgePlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgePlanStatus next)
    {
        switch (Status)
        {
            case AccessBadgePlanStatus.Draft:
                switch (next)
                {
                    case AccessBadgePlanStatus.Pending:
                    case AccessBadgePlanStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgePlanStatus.Pending:
                return next == AccessBadgePlanStatus.Suspended || next == AccessBadgePlanStatus.Closed;
            case AccessBadgePlanStatus.Active:
                return next == AccessBadgePlanStatus.Closed;
            case AccessBadgePlanStatus.Suspended:
                switch (next)
                {
                    case AccessBadgePlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"access badge plan"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte + (double)BatchSize, 2);
    }

    public AccessBadgePlan Copy()
    {
        return new AccessBadgePlan(Code, Name, BatchSize, Approved, Locale, Fte, Limit, Phone, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
