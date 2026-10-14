namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance template is in its life cycle.</summary>
public enum BalanceTemplateStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One balance template, with what the planners need to act on it.</summary>
public sealed class BalanceTemplate(string code, string name, string country, double fte, int graceDays, bool approved, string summary, string reviewer, BalanceTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Fte { get; set; } = fte;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public BalanceTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a balance template in status requested.</summary>
    public static BalanceTemplate NewBalanceTemplate(string code, string name)
    {
        return new BalanceTemplate(code, name, "", 0.0, 0, false, "", "", BalanceTemplateStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceTemplateStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceTemplateStatus next)
    {
        switch (Status)
        {
            case BalanceTemplateStatus.Requested:
                return next == BalanceTemplateStatus.Granted || next == BalanceTemplateStatus.Revoked;
            case BalanceTemplateStatus.Granted:
                switch (next)
                {
                    case BalanceTemplateStatus.Revoked:
                    case BalanceTemplateStatus.Expired:
                        return true;
                }
                return false;
            case BalanceTemplateStatus.Revoked:
                switch (next)
                {
                    case BalanceTemplateStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)GraceDays + Fte, 2);
    }

    public BalanceTemplate Copy()
    {
        return new BalanceTemplate(Code, Name, Country, Fte, GraceDays, Approved, Summary, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
