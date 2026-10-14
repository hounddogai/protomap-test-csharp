namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance agreement is in its life cycle.</summary>
public enum BalanceAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One balance agreement, with what the planners need to act on it.</summary>
public sealed class BalanceAgreement(string code, string label, bool archived, string jobCode, bool visible, double ratio, int attempts, BalanceAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public int Attempts { get; set; } = attempts;
    public BalanceAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a balance agreement in status draft.</summary>
    public static BalanceAgreement NewBalanceAgreement(string code, string label)
    {
        return new BalanceAgreement(code, label, false, "", false, 0.0, 0, BalanceAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceAgreementStatus next)
    {
        switch (Status)
        {
            case BalanceAgreementStatus.Draft:
                switch (next)
                {
                    case BalanceAgreementStatus.Pending:
                        return true;
                }
                return false;
            case BalanceAgreementStatus.Pending:
                return next == BalanceAgreementStatus.Active;
            case BalanceAgreementStatus.Active:
                return next == BalanceAgreementStatus.Suspended;
            case BalanceAgreementStatus.Suspended:
                switch (next)
                {
                    case BalanceAgreementStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ratio * (double)Attempts, 2);
    }

    public BalanceAgreement Copy()
    {
        return new BalanceAgreement(Code, Label, Archived, JobCode, Visible, Ratio, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
