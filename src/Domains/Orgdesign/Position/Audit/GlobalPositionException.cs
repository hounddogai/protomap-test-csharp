namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global position exception is in its life cycle.</summary>
public enum GlobalPositionExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global position exception. Its code is unique within a tenant.</summary>
public sealed class GlobalPositionException(string code, string name, int quantity, string grade, int priority, GlobalPositionExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public GlobalPositionExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a global position exception in status requested.</summary>
    public static GlobalPositionException NewGlobalPositionException(string code, string name)
    {
        return new GlobalPositionException(code, name, 0, "", 0, GlobalPositionExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPositionExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPositionExceptionStatus next)
    {
        switch (Status)
        {
            case GlobalPositionExceptionStatus.Requested:
                return next == GlobalPositionExceptionStatus.Granted;
            case GlobalPositionExceptionStatus.Granted:
                switch (next)
                {
                    case GlobalPositionExceptionStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalPositionExceptionStatus.Revoked:
                switch (next)
                {
                    case GlobalPositionExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPositionExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global position exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Quantity + (double)Priority, 2);
    }

    public GlobalPositionException Copy()
    {
        return new GlobalPositionException(Code, Name, Quantity, Grade, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
