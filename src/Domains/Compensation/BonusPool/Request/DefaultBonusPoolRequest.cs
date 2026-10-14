namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default bonus pool request is in its life cycle.</summary>
public enum DefaultBonusPoolRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default bonus pool request. Its code is unique within a tenant.</summary>
public sealed class DefaultBonusPoolRequest(string code, string name, double fte, int quantity, int priority, string summary, double cost, string reviewer, DefaultBonusPoolRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public string Summary { get; set; } = summary;
    public double Cost { get; set; } = cost;
    public string Reviewer { get; set; } = reviewer;
    public DefaultBonusPoolRequestStatus Status { get; set; } = status;

    /// <summary>Starts a default bonus pool request in status draft.</summary>
    public static DefaultBonusPoolRequest NewDefaultBonusPoolRequest(string code, string name)
    {
        return new DefaultBonusPoolRequest(code, name, 0.0, 0, 0, "", 0.0, "", DefaultBonusPoolRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBonusPoolRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBonusPoolRequestStatus next)
    {
        switch (Status)
        {
            case DefaultBonusPoolRequestStatus.Draft:
                return next == DefaultBonusPoolRequestStatus.Closed;
            case DefaultBonusPoolRequestStatus.Pending:
                return next == DefaultBonusPoolRequestStatus.Closed;
            case DefaultBonusPoolRequestStatus.Active:
                switch (next)
                {
                    case DefaultBonusPoolRequestStatus.Closed:
                        return true;
                }
                return false;
            case DefaultBonusPoolRequestStatus.Suspended:
                return next == DefaultBonusPoolRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultBonusPoolRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default bonus pool request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Cost + Fte, 2);
    }

    public DefaultBonusPoolRequest Copy()
    {
        return new DefaultBonusPoolRequest(Code, Name, Fte, Quantity, Priority, Summary, Cost, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
