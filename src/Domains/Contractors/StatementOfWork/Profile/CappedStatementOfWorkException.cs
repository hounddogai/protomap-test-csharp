namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped statement of work exception is in its life cycle.</summary>
public enum CappedStatementOfWorkExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped statement of work exception, with what the planners need to act on it.</summary>
public sealed class CappedStatementOfWorkException(string code, string name, double rate, string costCenter, List<string> tags, string channel, int createdAt, double discount, string employeeId, CappedStatementOfWorkExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public List<string> Tags { get; set; } = tags;
    public string Channel { get; set; } = channel;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public CappedStatementOfWorkExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a capped statement of work exception in status idle.</summary>
    public static CappedStatementOfWorkException NewCappedStatementOfWorkException(string code, string name)
    {
        return new CappedStatementOfWorkException(
            code,
            name,
            0.0,
            "",
            new List<string>(),
            "",
            0,
            0.0,
            "",
            CappedStatementOfWorkExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedStatementOfWorkExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedStatementOfWorkExceptionStatus next)
    {
        switch (Status)
        {
            case CappedStatementOfWorkExceptionStatus.Idle:
                switch (next)
                {
                    case CappedStatementOfWorkExceptionStatus.Ready:
                    case CappedStatementOfWorkExceptionStatus.Draining:
                        return true;
                }
                return false;
            case CappedStatementOfWorkExceptionStatus.Warming:
                return next == CappedStatementOfWorkExceptionStatus.Draining || next == CappedStatementOfWorkExceptionStatus.Stopped;
            case CappedStatementOfWorkExceptionStatus.Ready:
                return next == CappedStatementOfWorkExceptionStatus.Draining || next == CappedStatementOfWorkExceptionStatus.Stopped;
            case CappedStatementOfWorkExceptionStatus.Draining:
                switch (next)
                {
                    case CappedStatementOfWorkExceptionStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedStatementOfWorkExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped statement of work exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt + Discount, 2);
    }

    public CappedStatementOfWorkException Copy()
    {
        return new CappedStatementOfWorkException(
            Code,
            Name,
            Rate,
            CostCenter,
            Tags,
            Channel,
            CreatedAt,
            Discount,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
