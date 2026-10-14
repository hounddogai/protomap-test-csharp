namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work rule is in its life cycle.</summary>
public enum StatementOfWorkRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One statement of work rule, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkRule(string code, string name, string region, string phone, int windowDays, int endsAt, int tenureMonths, double amount, StatementOfWorkRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Phone { get; set; } = phone;
    public int WindowDays { get; set; } = windowDays;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public StatementOfWorkRuleStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work rule in status requested.</summary>
    public static StatementOfWorkRule NewStatementOfWorkRule(string code, string name)
    {
        return new StatementOfWorkRule(code, name, "", "", 0, 0, 0, 0.0, StatementOfWorkRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkRuleStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkRuleStatus.Requested:
                return next == StatementOfWorkRuleStatus.Revoked || next == StatementOfWorkRuleStatus.Expired;
            case StatementOfWorkRuleStatus.Granted:
                switch (next)
                {
                    case StatementOfWorkRuleStatus.Revoked:
                        return true;
                }
                return false;
            case StatementOfWorkRuleStatus.Revoked:
                return next == StatementOfWorkRuleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays + (double)TenureMonths, 2);
    }

    public StatementOfWorkRule Copy()
    {
        return new StatementOfWorkRule(Code, Name, Region, Phone, WindowDays, EndsAt, TenureMonths, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
