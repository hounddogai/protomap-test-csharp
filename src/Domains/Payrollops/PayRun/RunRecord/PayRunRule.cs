namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run rule is in its life cycle.</summary>
public enum PayRunRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pay run rule. Its code is unique within a tenant.</summary>
public sealed class PayRunRule(string code, string title, string country, int sequence, double cost, string employeeId, int endsAt, string team, string category, PayRunRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public int Sequence { get; set; } = sequence;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public int EndsAt { get; set; } = endsAt;
    public string Team { get; set; } = team;
    public string Category { get; set; } = category;
    public PayRunRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pay run rule in status requested.</summary>
    public static PayRunRule NewPayRunRule(string code, string title)
    {
        return new PayRunRule(code, title, "", 0, 0.0, "", 0, "", "", PayRunRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunRuleStatus next)
    {
        switch (Status)
        {
            case PayRunRuleStatus.Requested:
                return next == PayRunRuleStatus.Granted || next == PayRunRuleStatus.Expired;
            case PayRunRuleStatus.Granted:
                switch (next)
                {
                    case PayRunRuleStatus.Revoked:
                        return true;
                }
                return false;
            case PayRunRuleStatus.Revoked:
                return next == PayRunRuleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayRunRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt - (double)Sequence, 2);
    }

    public PayRunRule Copy()
    {
        return new PayRunRule(Code, Title, Country, Sequence, Cost, EmployeeId, EndsAt, Team, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
