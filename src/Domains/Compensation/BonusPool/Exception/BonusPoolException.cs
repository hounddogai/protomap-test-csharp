namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool exception is in its life cycle.</summary>
public enum BonusPoolExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A bonus pool exception. Its code is unique within a tenant.</summary>
public sealed class BonusPoolException(string code, string label, string externalRef, int attempts, double rate, int graceDays, BonusPoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public BonusPoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool exception in status draft.</summary>
    public static BonusPoolException NewBonusPoolException(string code, string label)
    {
        return new BonusPoolException(code, label, "", 0, 0.0, 0, BonusPoolExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolExceptionStatus next)
    {
        switch (Status)
        {
            case BonusPoolExceptionStatus.Draft:
                return next == BonusPoolExceptionStatus.Suspended;
            case BonusPoolExceptionStatus.Pending:
                return next == BonusPoolExceptionStatus.Closed;
            case BonusPoolExceptionStatus.Active:
                switch (next)
                {
                    case BonusPoolExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case BonusPoolExceptionStatus.Suspended:
                return next == BonusPoolExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BonusPoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool exception"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Attempts - Rate, 2);
    }

    public BonusPoolException Copy()
    {
        return new BonusPoolException(Code, Label, ExternalRef, Attempts, Rate, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
